using System.Collections.Generic;

namespace Bam.AI.Orchestration.Safety
{
    /// <summary>
    /// Result of safety validation
    /// </summary>
    public interface ISafetyResult
    {
        bool IsSafe { get; }
        SafetyLevel Level { get; }
        IEnumerable<ISafetyViolation> Violations { get; }
        string RecommendedAction { get; }
    }
}