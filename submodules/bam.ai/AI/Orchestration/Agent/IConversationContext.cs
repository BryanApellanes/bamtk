using System.Collections.Generic;
using Bam.AI.Orchestration.Core;

namespace Bam.AI.Orchestration.Agent
{
    /// <summary>
    /// Maintains conversation history and context.
    /// </summary>
    public interface IConversationContext
    {
        string ConversationId { get; }
        IEnumerable<IPrompt> PreviousPrompts { get; }
        IEnumerable<IAgentResponse> PreviousResponses { get; }
        IDictionary<string, object> SessionData { get; }

        void AddExchange(IPrompt prompt, IAgentResponse response);
        void Clear();
    }
}