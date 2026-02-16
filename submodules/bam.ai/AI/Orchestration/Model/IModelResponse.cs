using System.Collections.Generic;
using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Response from the language model
    /// </summary>
    public interface IModelResponse
    {
        string Content { get; }
        IEnumerable<IToolCall> ToolCalls { get; }
        FinishReason FinishReason { get; }
        IUsageMetrics Usage { get; }
    }
}