using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Model
{

    /// <summary>
    /// Concrete implementation of a conversation message.
    /// </summary>
    public class Message : IMessage
    {
        /// <summary>
        /// Gets the role of the message sender.
        /// </summary>
        public MessageRole Role { get; }


        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string Content { get; }


        /// <summary>
        /// Gets the tool calls in this message.
        /// </summary>
        /// <value>
        /// Tool calls made by the assistant. Only populated for assistant messages
        /// that invoke tools.
        /// </value>
        public IEnumerable<IToolCall> ToolCalls { get; }


        /// <summary>
        /// Gets the tool results in this message.
        /// </summary>
        /// <value>
        /// Results from tool executions. Only populated for tool messages that
        /// return results to the model.
        /// </value>
        public IEnumerable<IToolResult> ToolResults { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="role">The message role.</param>
        /// <param name="content">The message content.</param>
        /// <param name="toolCalls">Optional tool calls.</param>
        /// <param name="toolResults">Optional tool results.</param>
        public Message(
            MessageRole role,
            string content,
            IEnumerable<IToolCall> toolCalls = null,
            IEnumerable<IToolResult> toolResults = null)
        {
            Role = role;
            Content = content ?? string.Empty;
            ToolCalls = toolCalls ?? Enumerable.Empty<IToolCall>();
            ToolResults = toolResults ?? Enumerable.Empty<IToolResult>();
        }
    }
}