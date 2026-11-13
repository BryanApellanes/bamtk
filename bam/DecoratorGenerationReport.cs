namespace Bam
{
    /// <summary>
    /// The outcome of one <see cref="GenerateDecoratorCommand.Execute"/> run: the decorator files written and
    /// the services skipped, each with its reason. A run continues past a service it cannot generate, so a
    /// report can hold both.
    /// </summary>
    public class DecoratorGenerationReport
    {
        /// <summary>Initializes a new instance.</summary>
        /// <param name="written">The paths of the files written, in generation order.</param>
        /// <param name="skipped">The services no decorator was generated for.</param>
        public DecoratorGenerationReport(IReadOnlyList<string> written, IReadOnlyList<SkippedDecorator> skipped)
        {
            ArgumentNullException.ThrowIfNull(written);
            ArgumentNullException.ThrowIfNull(skipped);

            Written = written;
            Skipped = skipped;
        }

        /// <summary>Gets the paths of the decorator files written, in generation order.</summary>
        public IReadOnlyList<string> Written { get; }

        /// <summary>Gets the services no decorator was generated for, with reasons.</summary>
        public IReadOnlyList<SkippedDecorator> Skipped { get; }

        /// <summary>Gets a value indicating whether at least one decorator was written.</summary>
        public bool WroteAnything => Written.Count > 0;

        /// <summary>
        /// Gets why the run counts as failed, or null when it does not: it wrote nothing, or
        /// <paramref name="failOnSkip"/> is set and it skipped a service. The message counts the skips and does
        /// not repeat them; they are printed with their reasons beside it.
        /// </summary>
        /// <param name="failOnSkip">The config's <see cref="BamDecoratorGenerationConfig.FailOnSkip"/>.</param>
        /// <returns>The failure message, or null.</returns>
        public string? FailureMessage(bool failOnSkip)
        {
            if (!WroteAnything)
            {
                return $"No decorator was generated: {Skipped.Count} skipped, each listed above with its reason.";
            }

            if (failOnSkip && Skipped.Count > 0)
            {
                return $"{nameof(BamDecoratorGenerationConfig.FailOnSkip)} is set and {Skipped.Count} skipped (each is listed above with its reason). The {Written.Count} decorator file(s) written are on disk.";
            }

            return null;
        }
    }
}
