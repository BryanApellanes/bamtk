using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bam.AI.Orchestration.Agent;

namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// Selects appropriate tools based on analyzed intent
    /// </summary>
    public interface IToolSelector
    {
        Task<IEnumerable<ISelectedTool>> SelectToolsAsync(
            IPromptIntent intent,
            IToolRegistry registry,
            CancellationToken cancellationToken = default);
    }
}