using System;

namespace Bam.AI.Orchestration.Agent
{
    /// <summary>
    /// Concrete implementation of an extracted entity.
    /// </summary>
    public class Entity : IEntity
    {
        /// <summary>
        /// Gets the entity type (e.g., "date", "number", "email").
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the extracted value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the confidence score for this extraction.
        /// </summary>
        public double Confidence { get; }

        /// <summary>
        /// Gets the start position in the original text.
        /// </summary>
        public int StartPosition { get; }

        /// <summary>
        /// Gets the end position in the original text.
        /// </summary>
        public int EndPosition { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        public Entity(string type, string value, double confidence, int startPosition, int endPosition)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Confidence = Math.Clamp(confidence, 0.0, 1.0);
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
    }
}