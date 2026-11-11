namespace Bam
{
    /// <summary>
    /// Asks <c>bam generate decorator</c> to find the services itself: every public, closed interface in an
    /// assembly, each paired with its single public implementation from the same assembly. An interface with
    /// no public implementation, or with several, is reported as skipped unless <see cref="InterfaceOnly"/> is
    /// set, in which case it is decorated from the interface alone. An interface that a
    /// <see cref="BamDecoratorGenerationConfig.Services"/> entry or the single pair already names is passed
    /// over, whether that entry generated or was skipped, and a generated decorator compiled into the assembly
    /// is never counted as an implementation, so a config can be run again over its own output.
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
        /// decorated from the interface alone instead of being skipped. Defaults to false. When some types of
        /// the assembly could not be loaded, such an interface is skipped all the same: its implementation may
        /// be one of them.
        /// </summary>
        public bool InterfaceOnly { get; set; }
    }
}
