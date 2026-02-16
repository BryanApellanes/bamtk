namespace Bam.AI.Orchestration.Safety
{

    /// <summary>
    /// Concrete implementation of a safety result.
    /// </summary>
    public class SafetyResult : ISafetyResult
    {
        /// <summary>
        /// Gets whether the content is safe.
        /// </summary>
        public bool IsSafe { get; }


        /// <summary>
        /// Gets the safety level.
        /// </summary>
        public SafetyLevel Level { get; }


        /// <summary>
        /// Gets the detected violations.
        /// </summary>
        public IEnumerable<ISafetyViolation> Violations { get; }


        /// <summary>
        /// Gets the recommended action.
        /// </summary>
        public string RecommendedAction { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="SafetyResult"/> class.
        /// </summary>
        public SafetyResult(
            bool isSafe,
            SafetyLevel level,
            IEnumerable<ISafetyViolation> violations,
            string recommendedAction)
        {
            IsSafe = isSafe;
            Level = level;
            Violations = violations ?? Enumerable.Empty<ISafetyViolation>();
            RecommendedAction = recommendedAction ?? "Review manually";
        }
    }
}