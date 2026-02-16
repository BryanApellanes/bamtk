using System.Collections.Generic;

namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// Represents a tool call made during agent processing
    /// </summary>
    public interface IToolCall
    {
        string ToolId { get; }
        string ToolName { get; }
        IDictionary<string, object> Parameters { get; }
        IToolResult Result { get; }
    }
}