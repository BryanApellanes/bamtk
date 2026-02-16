using System.Threading;
using System.Threading.Tasks;
using Bam.AI.Orchestration.Core;
using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Safety
{
    /// <summary>
    /// Validates requests and responses for safety
    /// </summary>
    public interface ISafetyValidator
    {
        Task<ISafetyResult> ValidatePromptAsync(
            IPrompt prompt,
            CancellationToken cancellationToken = default);

        Task<ISafetyResult> ValidateResponseAsync(
            IAgentResponse response,
            CancellationToken cancellationToken = default);

        Task<ISafetyResult> ValidateToolCallAsync(
            ISelectedTool tool,
            CancellationToken cancellationToken = default);
    }
}