namespace Bam.AI.Orchestration.Tools
{

    /// <summary>
    /// Concrete implementation of a tool result.
    /// </summary>
    public class ToolResult : IToolResult
    {
        /// <summary>
        /// Gets the ID of the tool that was executed.
        /// </summary>
        public string ToolId { get; }


        /// <summary>
        /// Gets whether the execution was successful.
        /// </summary>
        public bool Success { get; }


        /// <summary>
        /// Gets the data returned by the tool.
        /// </summary>
        /// <value>
        /// The output data, or null if the execution failed.
        /// The type depends on the tool's output schema.
        /// </value>
        public object Data { get; }


        /// <summary>
        /// Gets the error message if execution failed.
        /// </summary>
        /// <value>
        /// A descriptive error message, or null if execution succeeded.
        /// </value>
        public string ErrorMessage { get; }


        /// <summary>
        /// Gets when the tool was executed.
        /// </summary>
        public DateTime ExecutedAt { get; }


        /// <summary>
        /// Gets how long the execution took.
        /// </summary>
        public TimeSpan ExecutionDuration { get; }


        /// <summary>
        /// Gets additional metadata about the execution.
        /// </summary>
        public IDictionary<string, object> Metadata { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolResult"/> class.
        /// </summary>
        public ToolResult(
            string toolId,
            bool success,
            object data,
            string errorMessage,
            DateTime executedAt,
            TimeSpan executionDuration,
            IDictionary<string, object> metadata = null)
        {
            ToolId = toolId ?? throw new ArgumentNullException(nameof(toolId));
            Success = success;
            Data = data;
            ErrorMessage = errorMessage;
            ExecutedAt = executedAt;
            ExecutionDuration = executionDuration;
            Metadata = metadata ?? new Dictionary<string, object>();
        }
    }
}