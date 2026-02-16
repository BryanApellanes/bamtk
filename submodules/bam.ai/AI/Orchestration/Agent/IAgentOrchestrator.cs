using System.Threading;
using System.Threading.Tasks;
using Bam.AI.Orchestration.Core;

namespace Bam.AI.Orchestration.Agent
{
    /// <summary>
    /// Main orchestrator that coordinates the AI agent's processing pipeline.
    /// </summary>
    public interface IAgentOrchestrator
    {
        Task<IAgentResponse> ProcessPromptAsync(
            IPrompt prompt,
            CancellationToken cancellationToken = default);

        Task<IAgentResponse> ProcessPromptWithContextAsync(
            IPrompt prompt,
            IConversationContext context,
            CancellationToken cancellationToken = default);
    }
}