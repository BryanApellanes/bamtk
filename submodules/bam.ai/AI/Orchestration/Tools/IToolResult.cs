using System;
using System.Collections.Generic;

namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// Result from a tool execution
    /// </summary>
    public interface IToolResult
    {
        string ToolId { get; }
        bool Success { get; }
        object Data { get; }
        string ErrorMessage { get; }
        DateTime ExecutedAt { get; }
        TimeSpan ExecutionDuration { get; }
        IDictionary<string, object> Metadata { get; }
    }
}