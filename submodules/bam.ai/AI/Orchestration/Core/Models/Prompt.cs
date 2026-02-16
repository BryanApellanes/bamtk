using System;
using System.Collections.Generic;
using System.Linq;

namespace Bam.AI.Orchestration.Core.Models
{
    /// <summary>
    /// Concrete implementation of a user prompt/message to the AI agent.
    /// </summary>
    /// <remarks>
    /// This class represents the complete input from a user, including the main
    /// content, metadata for tracking and personalization, and any file attachments.
    /// Instances are typically created by the application layer and passed to the
    /// orchestrator for processing.
    /// 
    /// Thread Safety: This class is immutable after construction and is thread-safe.
    /// </remarks>
    public class Prompt : IPrompt
    {
        /// <summary>
        /// Gets the text content of the prompt.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Gets the unique identifier of the user who created this prompt.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Gets the timestamp when this prompt was created.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets additional metadata associated with this prompt.
        /// </summary>
        public IDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Gets the file attachments included with this prompt.
        /// </summary>
        public IEnumerable<IAttachment> Attachments { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Prompt"/> class.
        /// </summary>
        public Prompt(
            string content,
            string userId,
            IDictionary<string, object> metadata = null,
            IEnumerable<IAttachment> attachments = null)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be null or empty", nameof(content));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId cannot be null or empty", nameof(userId));

            Content = content;
            UserId = userId;
            Timestamp = DateTime.UtcNow;
            Metadata = metadata ?? new Dictionary<string, object>();
            Attachments = attachments ?? Enumerable.Empty<IAttachment>();
        }

        /// <summary>
        /// Returns a string representation of this prompt for debugging purposes.
        /// </summary>
        public override string ToString()
        {
            var preview = Content.Length > 50
                ? Content.Substring(0, 47) + "..."
                : Content;
            return $"Prompt[User={UserId}, Time={Timestamp:u}, Content=\"{preview}\"]";
        }
    }
}