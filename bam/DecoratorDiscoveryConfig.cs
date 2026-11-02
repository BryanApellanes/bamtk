namespace Bam
{
    /// <summary>
    /// Asks <c>bam generate decorator</c> to find the services itself: every public, closed interface in an
    /// assembly, each paired with its single public implementation from the same assembly. An interface with
    /// no public implementation, or with several, is reported as skipped unless <see cref="InterfaceOnly"/> is
    /// set, in which case it is decorated from the interface alone.
    /// </summary>
    public class DecoratorDiscoveryConfig
    {
        /// <summary>
        /// Path to the compiled assembly to scan. Defaults to the config's
        /// <see cref="BamDecoratorGenerationConfig.AssemblyPath"/>.
        /// </summary>
        public string? AssemblyPath { get; set; }

        /// <summary>
        /// Only interfaces in this namespace, or a namespace below it, are decorated. Empty scans the whole
        /// assembly.
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// When true, an interface with no public implementation in the assembly, or with more than one, is
        /// decorated from the interface alone instead of being skipped. Defaults to false.
        /// </summary>
        public bool InterfaceOnly { get; set; }
    }
}
