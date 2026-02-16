using System.Collections.Generic;

namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Parameters for model generation
    /// </summary>
    public interface IModelParameters
    {
        double Temperature { get; }
        int MaxTokens { get; }
        double TopP { get; }
        double FrequencyPenalty { get; }
        double PresencePenalty { get; }
        IEnumerable<string> StopSequences { get; }
    }
}