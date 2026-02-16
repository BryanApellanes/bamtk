using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bam.AI.Orchestration.Agent;
using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Planning
{
    /// <summary>
    /// Creates execution plans for complex multi-step tasks
    /// </summary>
    public interface ITaskPlanner
    {
        Task<IExecutionPlan> CreatePlanAsync(
            IPromptIntent intent,
            IEnumerable<IToolDescriptor> availableTools,
            CancellationToken cancellationToken = default);
    }
}