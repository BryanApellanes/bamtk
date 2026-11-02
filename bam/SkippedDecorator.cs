namespace Bam
{
    /// <summary>
    /// A service <see cref="GenerateDecoratorCommand"/> did not generate a decorator for, and why. Part of a
    /// <see cref="DecoratorGenerationReport"/>.
    /// </summary>
    public class SkippedDecorator
    {
        /// <summary>Initializes a new instance.</summary>
        /// <param name="interfaceName">The service interface, as named in the config or as discovered.</param>
        /// <param name="implementationName">The implementation, or null when the interface was to be decorated alone.</param>
        /// <param name="reason">Why no decorator was generated.</param>
        public SkippedDecorator(string interfaceName, string? implementationName, string reason)
        {
            InterfaceName = interfaceName;
            ImplementationName = implementationName;
            Reason = reason;
        }

        /// <summary>Gets the service interface, as named in the config or as discovered.</summary>
        public string InterfaceName { get; }

        /// <summary>Gets the implementation, or null when the interface was to be decorated alone.</summary>
        public string? ImplementationName { get; }

        /// <summary>Gets why no decorator was generated.</summary>
        public string Reason { get; }

        /// <summary>Formats the skip as <c>Interface / Implementation: reason</c>.</summary>
        /// <returns>The formatted skip.</returns>
        public override string ToString()
        {
            return ImplementationName is null
                ? $"{InterfaceName}: {Reason}"
                : $"{InterfaceName} / {ImplementationName}: {Reason}";
        }
    }
}
