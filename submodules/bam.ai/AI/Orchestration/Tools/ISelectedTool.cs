using System.Collections.Generic;

namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// A tool selected for execution with its parameters
    /// </summary>
    public interface ISelectedTool
    {
        IToolDescriptor Descriptor { get; }
        IDictionary<string, object> Parameters { get; }
        int ExecutionOrder { get; }
        double ConfidenceScore { get; }
    }
}