using System;
using System.Collections.Generic;
using System.Linq;

namespace Bam.AI.Orchestration.Agent
{
    /// <summary>
    /// Concrete implementation of prompt intent.
    /// </summary>
    public class PromptIntent : IPromptIntent
    {
        /// <summary>
        /// Gets the classified intent type.
        /// </summary>
        public IntentType Type { get; }

        /// <summary>
        /// Gets the confidence score for this classification.
        /// </summary>
        /// <value>
        /// A value between 0.0 and 1.0 indicating classification confidence.
        /// Values above 0.7 are considered high confidence.
        /// </value>
        public double Confidence { get; }

        /// <summary>
        /// Gets the entities extracted from the prompt.
        /// </summary>
        public IEnumerable<IEntity> ExtractedEntities { get; }

        /// <summary>
        /// Gets whether this intent requires tool usage.
        /// </summary>
        public bool RequiresToolUse { get; }

        /// <summary>
        /// Gets a human-readable summary of the intent.
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PromptIntent"/> class.
        /// </summary>
        public PromptIntent(
            IntentType type,
            double confidence,
            IEnumerable<IEntity> extractedEntities,
            bool requiresToolUse,
            string summary)
        {
            Type = type;
            Confidence = Math.Clamp(confidence, 0.0, 1.0);
            ExtractedEntities = extractedEntities ?? Enumerable.Empty<IEntity>();
            RequiresToolUse = requiresToolUse;
            Summary = summary ?? string.Empty;
        }
    }
}