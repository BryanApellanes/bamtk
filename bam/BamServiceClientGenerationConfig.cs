using Bam.Generators;

namespace Bam
{
    /// <summary>
    /// Input for the <c>bam generate service-client</c> command: identifies the compiled assembly and
    /// <c>[WebService]</c> type to wrap, the client shape to emit, and where to write the generated source.
    /// Deserialized from a <c>--config</c> YAML/JSON file by the CLI's brokered argument provider, and usable
    /// programmatically via <see cref="ReadFrom(string)"/>. Mirrors the shape/lifecycle of
    /// <see cref="DaoRepoGenerationConfig"/>.
    /// </summary>
    public class BamServiceClientGenerationConfig
    {
        /// <summary>
        /// The default config file path (<c>./BamServiceClientGenerationConfig.yaml</c>) used when the command
        /// is invoked without an explicit <c>--config</c> path and by the <c>service-client-init</c> scaffolder.
        /// </summary>
        public static string DefaultFilePath = $"./{nameof(BamServiceClientGenerationConfig)}.yaml";

        /// <summary>
        /// Path to the compiled assembly that contains the service type. Loaded via
        /// <see cref="System.Reflection.Assembly.LoadFrom(string)"/>; the assembly's own dependencies must be
        /// resolvable (present alongside it). Required.
        /// </summary>
        public string AssemblyPath { get; set; } = null!;

        /// <summary>
        /// Fully-qualified name of the <c>[WebService]</c> type to generate a client for (as accepted by
        /// <see cref="System.Reflection.Assembly.GetType(string)"/>). Required.
        /// </summary>
        public string ServiceTypeName { get; set; } = null!;

        /// <summary>
        /// The generated client shape. <see cref="GenerationMode.Subclass"/> (default) requires the service's
        /// remotable methods to be <see langword="virtual"/>; <see cref="GenerationMode.Interface"/> implements
        /// <see cref="InterfaceTypeName"/> (or the <c>I{ServiceName}</c> convention).
        /// </summary>
        public GenerationMode Mode { get; set; } = GenerationMode.Subclass;

        /// <summary>
        /// Fully-qualified interface type to implement in <see cref="GenerationMode.Interface"/> mode. Optional;
        /// when omitted the generator falls back to the <c>I{ServiceName}</c> convention. Ignored in Subclass mode.
        /// </summary>
        public string? InterfaceTypeName { get; set; }

        /// <summary>
        /// Directory the generated <c>{ServiceName}Client.cs</c> is written to; created if it does not exist. Required.
        /// </summary>
        public string OutputDirectory { get; set; } = null!;

        /// <summary>
        /// Deserializes a configuration from the specified YAML or JSON file (format chosen by extension).
        /// </summary>
        /// <param name="path">Path to the config file.</param>
        /// <returns>The deserialized <see cref="BamServiceClientGenerationConfig"/>.</returns>
        public static BamServiceClientGenerationConfig ReadFrom(string path)
        {
            return ReadFrom(new FileInfo(path));
        }

        /// <summary>
        /// Deserializes a configuration from the specified YAML or JSON file (format chosen by extension).
        /// </summary>
        /// <param name="file">The config file.</param>
        /// <returns>The deserialized <see cref="BamServiceClientGenerationConfig"/>.</returns>
        public static BamServiceClientGenerationConfig ReadFrom(FileInfo file)
        {
            return file.FromFile<BamServiceClientGenerationConfig>();
        }
    }
}
