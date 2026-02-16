using System;
using System.Collections.Generic;
using System.Linq;
using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Core.Models
{
    /// <summary>
    /// Concrete implementation of an agent's response.
    /// </summary>
    public class AgentResponse : IAgentResponse
    {
        /// <summary>
        /// Gets the text content of the response.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Gets the timestamp when this response was generated.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the tool calls that were executed during processing.
        /// </summary>
        public IEnumerable<IToolCall> ToolCallsExecuted { get; }

        /// <summary>
        /// Gets the final status of the response generation.
        /// </summary>
        public ResponseStatus Status { get; }

        /// <summary>
        /// Gets additional metadata about the response.
        /// </summary>
        public IDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentResponse"/> class.
        /// </summary>
        public AgentResponse(
            string content,
            ResponseStatus status,
            IEnumerable<IToolCall> toolCallsExecuted = null,
            IDictionary<string, object> metadata = null)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Status = status;
            Timestamp = DateTime.UtcNow;
            ToolCallsExecuted = toolCallsExecuted ?? Enumerable.Empty<IToolCall>();
            Metadata = metadata ?? new Dictionary<string, object>();
        }
    }
}