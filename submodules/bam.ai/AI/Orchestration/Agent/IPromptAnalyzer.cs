using System.Threading;
using System.Threading.Tasks;
using Bam.AI.Orchestration.Core;

namespace Bam.AI.Orchestration.Agent
{
    /// <summary>
    /// Analyzes prompts to understand intent and extract entities.
    /// </summary>
    public interface IPromptAnalyzer
    {
        Task<IPromptIntent> AnalyzeAsync(
            IPrompt prompt,
            IConversationContext context,
            CancellationToken cancellationToken = default);
    }
}