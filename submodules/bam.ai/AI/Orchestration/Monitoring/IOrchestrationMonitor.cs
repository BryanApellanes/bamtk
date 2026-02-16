using Bam.AI.Orchestration.Core;
using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Monitoring
{
    public interface IOrchestrationMonitor
    {
        Task<IOrchestrationMetrics> GetMetricsAsync(DateTime startTime, DateTime endTime);
        void TrackError(Exception exception, string context);
        void TrackPrompt(IPrompt prompt);
        void TrackResponse(IAgentResponse response, TimeSpan duration);
        void TrackToolExecution(IToolCall toolCall);
    }
}