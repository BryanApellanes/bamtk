using System.Collections.Generic;

namespace Bam.AI.Orchestration.Agent
{
    /// <summary>
    /// Represents the understood intent from a prompt.
    /// </summary>
    public interface IPromptIntent
    {
        IntentType Type { get; }
        double Confidence { get; }
        IEnumerable<IEntity> ExtractedEntities { get; }
        bool RequiresToolUse { get; }
        string Summary { get; }
    }
}