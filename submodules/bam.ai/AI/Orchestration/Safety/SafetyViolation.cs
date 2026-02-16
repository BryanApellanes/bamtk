namespace Bam.AI.Orchestration.Safety
{

    /// <summary>
    /// Concrete implementation of a safety violation.
    /// </summary>
    public class SafetyViolation : ISafetyViolation
    {
        /// <summary>
        /// Gets the violation category.
        /// </summary>
        public string Category { get; }


        /// <summary>
        /// Gets the violation description.
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// Gets the confidence score.
        /// </summary>
        public double Confidence { get; }


        /// <summary>
        /// Gets the severity level.
        /// </summary>
        public SafetyLevel Severity { get; }

        public string Type { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="SafetyViolation"/> class.
        /// </summary>
        public SafetyViolation(
            string category,
            string description,
            double confidence,
            SafetyLevel severity)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Confidence = Math.Clamp(confidence, 0.0, 1.0);
            Severity = severity;
        }
    }
}