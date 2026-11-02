using Bam.Console;
using Bam.DependencyInjection;
using Bam.Shell;

namespace Bam
{
    /// <summary>
    /// The <c>generate</c> command menu for the bam CLI — houses code-generation commands. Discovered
    /// automatically by the menu scanner via <see cref="ConsoleMenu"/>; dispatched through the brokered
    /// menu path (<c>MenuCommandRunner</c>), independent of the unimplemented <c>CompositeCommandRunner</c>.
    /// </summary>
    /// <remarks>
    /// Thin plumbing only: each command reads its deserialized config, delegates to a console-independent action
    /// type, and reports the outcome. Because the brokered argument provider binds one complex parameter from a
    /// single <c>--config &lt;path&gt;</c> file, the invocation is
    /// <c>bam generate ServiceClient --config &lt;path.yaml&gt;</c> (the command token is the method name /
    /// <c>sc</c> acronym; the framework derives dispatch keys from those).
    /// </remarks>
    [ConsoleMenu("generate")]
    public class GenerateMenuContainer : ConsoleMenuContainer
    {
        /// <summary>Initializes the container with the CLI <see cref="ServiceRegistry"/>.</summary>
        /// <param name="serviceRegistry">The registry the console menu system supplies for dependency resolution.</param>
        public GenerateMenuContainer(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        /// <summary>No extra registrations are required; the action composes the generator via its default ctor.</summary>
        /// <param name="serviceRegistry">The registry to configure.</param>
        /// <returns>The unchanged <paramref name="serviceRegistry"/>.</returns>
        public override ServiceRegistry Configure(ServiceRegistry serviceRegistry)
        {
            return serviceRegistry;
        }

        /// <summary>
        /// Generates a strongly-typed client for the <c>[WebService]</c> type described by the <c>--config</c> file
        /// (e.g. <c>bam generate ServiceClient --config ./client.yaml</c>) and writes <c>{ServiceName}Client.cs</c>
        /// to the configured output directory. On failure (invalid config, unresolved type, or non-virtual methods
        /// in Subclass mode) the thrown exception propagates to the CLI broker, which reports it and exits non-zero.
        /// </summary>
        /// <param name="config">The generation config, deserialized from the <c>--config</c> YAML/JSON file by the brokered argument provider.</param>
        [ConsoleCommand("service-client", "Generate a strongly-typed BAM service client from a --config YAML/JSON file")]
        public void ServiceClient(BamServiceClientGenerationConfig config)
        {
            string outputPath = new GenerateServiceClientCommand().Execute(config);
            Message.PrintLine("Generated service client: {0}", outputPath);
        }

        /// <summary>
        /// Writes a starter service-client generation config (YAML) to
        /// <see cref="BamServiceClientGenerationConfig.DefaultFilePath"/> so a user can fill it in and pass it to
        /// <see cref="ServiceClient"/>. Mirrors the <c>initConfig</c> convention of the other generator tools.
        /// </summary>
        [ConsoleCommand("service-client-init", "Write a starter service-client generation config (YAML) to the current directory")]
        public void ServiceClientInit()
        {
            const string template =
                "# bam generate service-client configuration\n" +
                "AssemblyPath: ./MyService.dll                 # compiled assembly containing the [WebService] type\n" +
                "ServiceTypeName: My.Namespace.MyService       # fully-qualified [WebService] type name\n" +
                "Mode: Subclass                                # Subclass (requires virtual methods) or Interface\n" +
                "InterfaceTypeName:                            # Interface mode only; omit for the I{ServiceName} convention\n" +
                "OutputDirectory: ./Generated_Clients          # where {ServiceName}Client.cs is written\n";

            File.WriteAllText(BamServiceClientGenerationConfig.DefaultFilePath, template);
            Message.PrintLine("Wrote service-client generation config: {0}", BamServiceClientGenerationConfig.DefaultFilePath);
        }

        /// <summary>
        /// Generates a strongly-typed decorator, and the extension methods for subscribing to it, for every
        /// service described by the <c>--config</c> file (e.g. <c>bam generate Decorator --config ./decorator.yaml</c>)
        /// and writes each <c>{ImplementationName}Decorator.cs</c> to the configured output directory. Every file
        /// written and every service skipped (with its reason) is printed. An invalid config propagates to the CLI
        /// broker as a thrown exception, which reports it and exits non-zero; so does a run that writes nothing.
        /// </summary>
        /// <param name="config">The generation config, deserialized from the <c>--config</c> YAML/JSON file by the brokered argument provider.</param>
        [ConsoleCommand("decorator", "Generate strongly-typed service decorators from a --config YAML/JSON file")]
        public void Decorator(BamDecoratorGenerationConfig config)
        {
            DecoratorGenerationReport report = new GenerateDecoratorCommand().Execute(config);
            foreach (string outputPath in report.Written)
            {
                Message.PrintLine("Generated decorator: {0}", outputPath);
            }

            foreach (SkippedDecorator skipped in report.Skipped)
            {
                Message.PrintLine("Skipped {0}", skipped.ToString());
            }

            if (!report.WroteAnything)
            {
                throw new InvalidOperationException(
                    "No decorator was generated. " + string.Join(" ", report.Skipped.Select(skipped => skipped.ToString())));
            }
        }

        /// <summary>
        /// Writes a starter decorator generation config (YAML) to
        /// <see cref="BamDecoratorGenerationConfig.DefaultFilePath"/> so a user can fill it in and pass it to
        /// <see cref="Decorator"/>. Mirrors <see cref="ServiceClientInit"/>.
        /// </summary>
        [ConsoleCommand("decorator-init", "Write a starter decorator generation config (YAML) to the current directory")]
        public void DecoratorInit()
        {
            File.WriteAllText(BamDecoratorGenerationConfig.DefaultFilePath, DecoratorConfigTemplate);
            Message.PrintLine("Wrote decorator generation config: {0}", BamDecoratorGenerationConfig.DefaultFilePath);
        }

        /// <summary>
        /// The starter config <see cref="DecoratorInit"/> writes. Exposed so a test can prove it deserializes
        /// into a <see cref="BamDecoratorGenerationConfig"/>.
        /// </summary>
        public const string DecoratorConfigTemplate =
            "# bam generate decorator configuration\n" +
            "AssemblyPath: ./MyService.dll                     # compiled assembly containing the service types; the default for every service below\n" +
            "InterfaceTypeName: My.Namespace.IMyService        # fully-qualified service interface\n" +
            "ImplementationTypeName: My.Namespace.MyService    # fully-qualified type that implements it; leave empty to decorate the interface alone\n" +
            "ImplementationAssemblyPath:                       # only when the implementation is in another assembly\n" +
            "OutputDirectory: ./Generated_Decorators           # where each {ImplementationName}Decorator.cs is written\n" +
            "RegistryExtensions: true                          # false generates decorators for another container (no ServiceRegistry extension methods)\n" +
            "TemplateDirectory:                                # directory of Handlebars templates overriding the built-in ones; empty consults ./Templates\n" +
            "ProbeDirectories: []                              # directories searched for dependencies the runtime cannot find next to the assemblies\n" +
            "Services: []                                      # more services, one entry each, for example:\n" +
            "#  - Interface: My.Namespace.IOtherService\n" +
            "#    Implementation: My.Namespace.OtherService    # leave out to decorate the interface alone\n" +
            "#    AssemblyPath:                                # this entry's assembly, when it is not AssemblyPath\n" +
            "#    ImplementationAssemblyPath:\n" +
            "#Discover:                                        # or decorate every public interface in an assembly, paired with its single public implementation\n" +
            "#  AssemblyPath:                                  # defaults to AssemblyPath\n" +
            "#  Namespace: My.Namespace                        # only interfaces in this namespace and below\n" +
            "#  InterfaceOnly: false                           # true decorates an interface with no (or several) implementations from the interface alone\n";
    }
}
