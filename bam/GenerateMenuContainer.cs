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
    }
}
