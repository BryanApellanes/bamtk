namespace Bam
{
    /// <summary>
    /// Input for the <c>bam generate decorator</c> command: the compiled assemblies holding the services to
    /// decorate, which services (listed, or discovered from an assembly), how to generate them, and where to
    /// write the decorators. Deserialized from a <c>--config</c> YAML/JSON file by the CLI's brokered argument
    /// provider, and usable programmatically via <see cref="ReadFrom(string)"/>. Mirrors the shape/lifecycle of
    /// <see cref="BamServiceClientGenerationConfig"/>.
    /// </summary>
    /// <remarks>
    /// A run generates every service it is given: the single pair named by <see cref="InterfaceTypeName"/> /
    /// <see cref="ImplementationTypeName"/> (the original, single-service shape), each entry of
    /// <see cref="Services"/>, and whatever <see cref="Discover"/> finds. At least one source is required.
    /// </remarks>
    public class BamDecoratorGenerationConfig
    {
        /// <summary>
        /// The default config file path (<c>./BamDecoratorGenerationConfig.yaml</c>) used when the command is
        /// invoked without an explicit <c>--config</c> path and by the <c>decorator-init</c> scaffolder.
        /// </summary>
        public static string DefaultFilePath = $"./{nameof(BamDecoratorGenerationConfig)}.yaml";

        /// <summary>
        /// Path to the compiled assembly that contains the service interface(s). Loaded via
        /// <see cref="System.Runtime.Loader.AssemblyLoadContext.LoadFromAssemblyPath(string)"/>; dependencies are resolved from the
        /// assembly's own directory and from <see cref="ProbeDirectories"/>. Required unless every
        /// <see cref="Services"/> entry and <see cref="Discover"/> name their own assembly.
        /// </summary>
        public string AssemblyPath { get; set; } = null!;

        /// <summary>
        /// Fully-qualified name of a service interface to decorate (as accepted by
        /// <see cref="System.Reflection.Assembly.GetType(string)"/>). Optional when <see cref="Services"/> or
        /// <see cref="Discover"/> supplies the services.
        /// </summary>
        public string InterfaceTypeName { get; set; } = null!;

        /// <summary>
        /// Fully-qualified name of the implementation type the <see cref="InterfaceTypeName"/> decorator wraps.
        /// It must implement the interface. Leave empty to decorate the interface alone.
        /// </summary>
        public string ImplementationTypeName { get; set; } = null!;

        /// <summary>
        /// Path to the compiled assembly that contains <see cref="ImplementationTypeName"/>, when it is not the
        /// one at <see cref="AssemblyPath"/>. Also the default for <see cref="Services"/> entries that name no
        /// assembly of their own. Optional.
        /// </summary>
        public string? ImplementationAssemblyPath { get; set; }

        /// <summary>
        /// Directory the generated <c>{ImplementationName}Decorator.cs</c> files are written to; created if it
        /// does not exist. Required.
        /// </summary>
        public string OutputDirectory { get; set; } = null!;

        /// <summary>
        /// Further services to decorate, one entry each. Entries default to <see cref="AssemblyPath"/> and
        /// <see cref="ImplementationAssemblyPath"/> when they name no assembly of their own.
        /// </summary>
        public List<DecoratorServiceConfig> Services { get; set; } = new List<DecoratorServiceConfig>();

        /// <summary>
        /// When set, the public interfaces of an assembly are discovered and decorated, each paired with its
        /// single public implementation. See <see cref="DecoratorDiscoveryConfig"/>.
        /// </summary>
        public DecoratorDiscoveryConfig? Discover { get; set; }

        /// <summary>
        /// Directories searched, by file name, for dependencies of the loaded assemblies that are not next to
        /// them. A dependency beside the assembly that needs it always wins over a same-named file here.
        /// Optional; each directory named must exist. Relative paths resolve against the current directory.
        /// </summary>
        public List<string> ProbeDirectories { get; set; } = new List<string>();

        /// <summary>
        /// A directory of Handlebars templates that override the generator's built-in ones (a
        /// <c>Decorator.hbs</c> there replaces the shipped decorator template). Empty consults <c>./Templates</c>
        /// under the current directory, as the generator does by default. Must exist when set.
        /// </summary>
        public string? TemplateDirectory { get; set; }

        /// <summary>
        /// Whether generated decorators include the <c>ServiceRegistry</c> extension methods. Defaults to true.
        /// False generates decorators that reference the <c>bam.generators.decorators.runtime</c> package alone,
        /// for hosts on another container.
        /// </summary>
        public bool RegistryExtensions { get; set; } = true;

        /// <summary>
        /// Whether a run that skips any service fails. Defaults to false, where only a run that writes nothing
        /// fails. Set it in a build so one mistyped service stops the build. Every file written and every skip
        /// is still printed first, and the files written stay on disk. What <see cref="Discover"/> passes over
        /// counts too (an interface with no or several implementations, a namespace with no interface); narrow
        /// it with <see cref="DecoratorDiscoveryConfig.Namespace"/> or set
        /// <see cref="DecoratorDiscoveryConfig.InterfaceOnly"/>.
        /// </summary>
        public bool FailOnSkip { get; set; }

        /// <summary>
        /// Lists every explicitly named service: the single pair from <see cref="InterfaceTypeName"/> /
        /// <see cref="ImplementationTypeName"/> when set, followed by <see cref="Services"/>. Blank
        /// implementation names read as "decorate the interface alone".
        /// </summary>
        /// <returns>The services, in config order.</returns>
        public IReadOnlyList<DecoratorServiceConfig> GetServices()
        {
            List<DecoratorServiceConfig> services = new List<DecoratorServiceConfig>();
            if (!string.IsNullOrWhiteSpace(InterfaceTypeName))
            {
                services.Add(new DecoratorServiceConfig
                {
                    Interface = InterfaceTypeName,
                    Implementation = string.IsNullOrWhiteSpace(ImplementationTypeName) ? null : ImplementationTypeName
                });
            }

            if (Services is not null)
            {
                services.AddRange(Services);
            }

            return services;
        }

        /// <summary>
        /// Deserializes a configuration from the specified YAML or JSON file (format chosen by extension).
        /// </summary>
        /// <param name="path">Path to the config file.</param>
        /// <returns>The deserialized <see cref="BamDecoratorGenerationConfig"/>.</returns>
        public static BamDecoratorGenerationConfig ReadFrom(string path)
        {
            return ReadFrom(new FileInfo(path));
        }

        /// <summary>
        /// Deserializes a configuration from the specified YAML or JSON file (format chosen by extension).
        /// </summary>
        /// <param name="file">The config file.</param>
        /// <returns>The deserialized <see cref="BamDecoratorGenerationConfig"/>.</returns>
        public static BamDecoratorGenerationConfig ReadFrom(FileInfo file)
        {
            return file.FromFile<BamDecoratorGenerationConfig>();
        }
    }
}
