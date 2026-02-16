using System;
using System.Collections.Generic;

namespace Bam.AI.Orchestration.Safety
{
    /// <summary>
    /// Exception thrown when a safety violation is detected.
    /// </summary>
    public class SafetyViolationException : Exception
    {
        /// <summary>
        /// Gets the safety violations that triggered this exception.
        /// </summary>
        public IEnumerable<ISafetyViolation> Violations { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafetyViolationException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="violations">The violations that were detected.</param>
        public SafetyViolationException(string message, IEnumerable<ISafetyViolation> violations)
            : base(message)
        {
            Violations = violations ?? new List<ISafetyViolation>();
        }
    }
}