using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// Executes tool calls and returns results
    /// </summary>
    public interface IToolExecutor
    {
        Task<IToolResult> ExecuteAsync(
            ISelectedTool tool,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<IToolResult>> ExecuteBatchAsync(
            IEnumerable<ISelectedTool> tools,
            ExecutionMode mode = ExecutionMode.Sequential,
            CancellationToken cancellationToken = default);
    }
}