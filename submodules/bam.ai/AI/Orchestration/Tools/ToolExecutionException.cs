namespace Bam.AI.Orchestration.Tools
{

    /// <summary>
    /// Exception thrown when a tool execution fails.
    /// </summary>
    public class ToolExecutionException : Exception
    {
        /// <summary>
        /// Gets the tool ID that failed.
        /// </summary>
        public string ToolId { get; }


        /// <summary>
        /// Gets additional error details.
        /// </summary>
        public IDictionary<string, object> ErrorDetails { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolExecutionException"/> class.
        /// </summary>
        public ToolExecutionException(
            string toolId,
            string message,
            Exception innerException = null,
            IDictionary<string, object> errorDetails = null)
            : base(message, innerException)
        {
            ToolId = toolId;
            ErrorDetails = errorDetails ?? new Dictionary<string, object>();
        }
    }
}