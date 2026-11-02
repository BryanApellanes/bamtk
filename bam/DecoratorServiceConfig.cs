namespace Bam
{
    /// <summary>
    /// One service to decorate in a <see cref="BamDecoratorGenerationConfig"/>: the service interface and,
    /// optionally, the implementation its decorator wraps. An entry without an <see cref="Implementation"/>
    /// decorates the interface alone (a decorator typed on the interface, named <c>I{Service}Decorator</c>).
    /// </summary>
    public class DecoratorServiceConfig
    {
        /// <summary>
        /// Fully-qualified name of the service interface the decorator implements (as accepted by
        /// <see cref="System.Reflection.Assembly.GetType(string)"/>). Required.
        /// </summary>
        public string Interface { get; set; } = null!;

        /// <summary>
        /// Fully-qualified name of the implementation type the decorator wraps. It must implement
        /// <see cref="Interface"/>. Leave empty to decorate the interface alone.
        /// </summary>
        public string? Implementation { get; set; }

        /// <summary>
        /// Path to the compiled assembly holding <see cref="Interface"/>, when it is not the config's
        /// <see cref="BamDecoratorGenerationConfig.AssemblyPath"/>. Optional.
        /// </summary>
        public string? AssemblyPath { get; set; }

        /// <summary>
        /// Path to the compiled assembly holding <see cref="Implementation"/>, when it is not the assembly the
        /// interface comes from. Optional. When this entry sets its own <see cref="AssemblyPath"/>, the config's
        /// <see cref="BamDecoratorGenerationConfig.ImplementationAssemblyPath"/> does not apply to it.
        /// </summary>
        public string? ImplementationAssemblyPath { get; set; }
    }
}
