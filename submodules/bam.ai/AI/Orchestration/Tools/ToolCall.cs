namespace Bam.AI.Orchestration.Tools
{

    /// <summary>
    /// Concrete implementation of a tool call record.
    /// </summary>
    public class ToolCall : IToolCall
    {
        /// <summary>
        /// Gets the tool ID.
        /// </summary>
        public string ToolId { get; }


        /// <summary>
        /// Gets the tool name.
        /// </summary>
        public string ToolName { get; }


        /// <summary>
        /// Gets the parameters that were passed to the tool.
        /// </summary>
        public IDictionary<string, object> Parameters { get; }


        /// <summary>
        /// Gets the result of the tool execution.
        /// </summary>
        public IToolResult Result { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolCall"/> class.
        /// </summary>
        public ToolCall(
            string toolId,
            string toolName,
            IDictionary<string, object> parameters,
            IToolResult result)
        {
            ToolId = toolId ?? throw new ArgumentNullException(nameof(toolId));
            ToolName = toolName ?? throw new ArgumentNullException(nameof(toolName));
            Parameters = parameters ?? new Dictionary<string, object>();
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }
    }
}