using System.Collections.Generic;

namespace Bam.AI.Orchestration.Planning
{
    /// <summary>
    /// A plan for executing a complex task
    /// </summary>
    public interface IExecutionPlan
    {
        string PlanId { get; }
        IEnumerable<IExecutionStep> Steps { get; }
        IDictionary<string, object> SharedContext { get; }
        double ConfidenceScore { get; }
    }
}