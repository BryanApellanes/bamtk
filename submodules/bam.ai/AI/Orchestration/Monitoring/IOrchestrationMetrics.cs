namespace Bam.AI.Orchestration.Monitoring
{
    public interface IOrchestrationMetrics
    {
        TimeSpan AverageResponseTime { get; }
        int FailedResponses { get; }
        IDictionary<string, int> IntentDistribution { get; }
        int SuccessfulResponses { get; }
        IDictionary<string, int> ToolUsageCount { get; }
        int TotalPrompts { get; }
    }
}