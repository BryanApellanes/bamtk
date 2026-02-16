using System;
using System.Collections.Generic;
using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Core
{
    /// <summary>
    /// Represents the AI agent's response to a prompt
    /// </summary>
    public interface IAgentResponse
    {
        string Content { get; }
        DateTime Timestamp { get; }
        IEnumerable<IToolCall> ToolCallsExecuted { get; }
        ResponseStatus Status { get; }
        IDictionary<string, object> Metadata { get; }
    }
}