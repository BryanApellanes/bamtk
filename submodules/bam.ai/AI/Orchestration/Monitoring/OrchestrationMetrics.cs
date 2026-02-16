namespace Bam.AI.Orchestration.Monitoring
{

    /// <summary>
    /// Concrete implementation of orchestration metrics.
    /// </summary>
    public class OrchestrationMetrics : IOrchestrationMetrics
    // : IOrchestrationMetrics
    {
        /// <summary>
        /// Gets the total number of prompts processed.
        /// </summary>
        public int TotalPrompts { get; }


        /// <summary>
        /// Gets the number of successful responses.
        /// </summary>
        public int SuccessfulResponses { get; }


        /// <summary>
        /// Gets the number of failed responses.
        /// </summary>
        public int FailedResponses { get; }


        /// <summary>
        /// Gets the average response time.
        /// </summary>
        public TimeSpan AverageResponseTime { get; }


        /// <summary>
        /// Gets the count of how many times each tool was used.
        /// </summary>
        public IDictionary<string, int> ToolUsageCount { get; }


        /// <summary>
        /// Gets the distribution of intent types.
        /// </summary>
        public IDictionary<string, int> IntentDistribution { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationMetrics"/> class.
        /// </summary>
        public OrchestrationMetrics(
            int totalPrompts,
            int successfulResponses,
            int failedResponses,
            TimeSpan averageResponseTime,
            IDictionary<string, int> toolUsageCount,
            IDictionary<string, int> intentDistribution)
        {
            TotalPrompts = totalPrompts;
            SuccessfulResponses = successfulResponses;
            FailedResponses = failedResponses;
            AverageResponseTime = averageResponseTime;
            ToolUsageCount = toolUsageCount ?? new Dictionary<string, int>();
            IntentDistribution = intentDistribution ?? new Dictionary<string, int>();
        }
    }
}