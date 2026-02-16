using System;
using System.Collections.Generic;
using System.Linq;
using Bam.AI.Orchestration.Core;

namespace Bam.AI.Orchestration.Agent
{
    /// <summary>
    /// Maintains conversation history and context for multi-turn dialogues.
    /// </summary>
    public class ConversationContext : IConversationContext
    {
        private readonly List<IPrompt> _prompts = new();
        private readonly List<IAgentResponse> _responses = new();

        /// <summary>
        /// Gets the unique identifier for this conversation.
        /// </summary>
        /// <value>
        /// A unique ID used to track and correlate conversation exchanges.
        /// Typically a GUID or database key.
        /// </value>
        public string ConversationId { get; }

        /// <summary>
        /// Gets all previous prompts in this conversation.
        /// </summary>
        /// <value>
        /// An ordered collection of prompts, from oldest to newest.
        /// Does not include the current prompt being processed.
        /// </value>
        public IEnumerable<IPrompt> PreviousPrompts => _prompts.AsReadOnly();

        /// <summary>
        /// Gets all previous responses in this conversation.
        /// </summary>
        /// <value>
        /// An ordered collection of responses, from oldest to newest.
        /// The count should match <see cref="PreviousPrompts"/>.
        /// </value>
        public IEnumerable<IAgentResponse> PreviousResponses => _responses.AsReadOnly();

        /// <summary>
        /// Gets session-specific data for this conversation.
        /// </summary>
        /// <value>
        /// A dictionary for storing arbitrary session state such as:
        /// - User preferences (language, verbosity, etc.)
        /// - Application state
        /// - Temporary working data
        /// - Feature flags
        /// This data persists across exchanges within the conversation.
        /// </value>
        public IDictionary<string, object> SessionData { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationContext"/> class.
        /// </summary>
        /// <param name="conversationId">The unique conversation identifier.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="conversationId"/> is null or empty.
        /// </exception>
        public ConversationContext(string conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
                throw new ArgumentException("ConversationId cannot be null or empty", nameof(conversationId));

            ConversationId = conversationId;
            SessionData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Adds a prompt-response exchange to the conversation history.
        /// </summary>
        /// <param name="prompt">The user's prompt.</param>
        /// <param name="response">The agent's response.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="prompt"/> or <paramref name="response"/> is null.
        /// </exception>
        public void AddExchange(IPrompt prompt, IAgentResponse response)
        {
            if (prompt == null)
                throw new ArgumentNullException(nameof(prompt));
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            _prompts.Add(prompt);
            _responses.Add(response);

            // Optional: Implement automatic pruning
            const int maxExchanges = 50;
            if (_prompts.Count > maxExchanges)
            {
                _prompts.RemoveAt(0);
                _responses.RemoveAt(0);
            }
        }

        /// <summary>
        /// Clears all conversation history and session data.
        /// </summary>
        /// <remarks>
        /// This method removes all prompts, responses, and session data, effectively
        /// resetting the conversation to a fresh state. The conversation ID is retained.
        /// 
        /// Use this when:
        /// - The user explicitly requests to start fresh
        /// - Switching to a completely different topic
        /// - Implementing a "reset" feature
        /// </remarks>
        public void Clear()
        {
            _prompts.Clear();
            _responses.Clear();
            SessionData.Clear();
        }
    }
}