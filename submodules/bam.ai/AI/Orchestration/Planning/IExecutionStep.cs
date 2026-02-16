using System.Collections.Generic;
using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Planning
{
    /// <summary>
    /// A single step in an execution plan
    /// </summary>
    public interface IExecutionStep
    {
        int Order { get; }
        string Description { get; }
        ISelectedTool Tool { get; }
        IEnumerable<string> DependsOn { get; } // Step IDs this depends on
        IExecutionCondition Condition { get; }
    }
}