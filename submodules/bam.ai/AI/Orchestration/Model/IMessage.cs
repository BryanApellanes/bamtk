using System.Collections.Generic;
using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// A message in the conversation
    /// </summary>
    public interface IMessage
    {
        MessageRole Role { get; }
        string Content { get; }
        IEnumerable<IToolCall> ToolCalls { get; }
        IEnumerable<IToolResult> ToolResults { get; }
    }
}