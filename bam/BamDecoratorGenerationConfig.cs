namespace Bam
{
    /// <summary>
    /// Input for the <c>bam generate decorator</c> command: identifies the compiled assembly holding a service
    /// interface and its implementation, and where to write the generated decorator. Deserialized from a
    /// <c>--config</c> YAML/JSON file by the CLI's brokered argument provider, and usable programmatically via
    /// <see cref="ReadFrom(string)"/>. Mirrors the shape/lifecycle of
    /// <see cref="BamServiceClientGenerationConfig"/>.
    /// </summary>
    public class BamDecoratorGenerationConfig
    {
        /// <summary>
        /// The default config file path (<c>./BamDecoratorGenerationConfig.yaml</c>) used when the command is
        /// invoked without an explicit <c>--config</c> path and by the <c>decorator-init</c> scaffolder.
        /// </summary>
        public static string DefaultFilePath = $"./{nameof(BamDecoratorGenerationConfig)}.yaml";

        /// <summary>
        /// Path to the compiled assembly that contains the service interface. Loaded via
        /// <see cref="System.Reflection.Assembly.LoadFrom(string)"/>; the assembly's own dependencies must be
        /// resolvable (present alongside it). Required.
        /// </summary>
        public string AssemblyPath { get; set; } = null!;

        /// <summary>
        /// Fully-qualified name of the service interface the decorator implements (as accepted by
        /// <see cref="System.Reflection.Assembly.GetType(string)"/>). Required.
        /// </summary>
        public string InterfaceTypeName { get; set; } = null!;

        /// <summary>
        /// Fully-qualified name of the implementation type the decorator wraps. It must implement
        /// <see cref="InterfaceTypeName"/>. Required.
        /// </summary>
        public string ImplementationTypeName { get; set; } = null!;

        /// <summary>
        /// Path to the compiled assembly that contains the implementation type, when it is not the one at
        /// <see cref="AssemblyPath"/>. Optional.
        /// </summary>
        public string? ImplementationAssemblyPath { get; set; }

        /// <summary>
        /// Directory the generated <c>{ImplementationName}Decorator.cs</c> is written to; created if it does
        /// not exist. Required.
        /// </summary>
        public string OutputDirectory { get; set; } = null!;

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
