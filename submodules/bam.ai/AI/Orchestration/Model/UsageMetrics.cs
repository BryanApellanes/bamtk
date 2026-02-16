namespace Bam.AI.Orchestration.Model
{

    /// <summary>
    /// Concrete implementation of usage metrics.
    /// </summary>
    public class UsageMetrics : IUsageMetrics
    {
        /// <summary>
        /// Gets the number of tokens in the prompt.
        /// </summary>
        public int PromptTokens { get; }


        /// <summary>
        /// Gets the number of tokens in the completion.
        /// </summary>
        public int CompletionTokens { get; }


        /// <summary>
        /// Gets the total number of tokens used.
        /// </summary>
        public int TotalTokens { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="UsageMetrics"/> class.
        /// </summary>
        public UsageMetrics(int promptTokens, int completionTokens, int totalTokens)
        {
            PromptTokens = promptTokens;
            CompletionTokens = completionTokens;
            TotalTokens = totalTokens;
        }
    }
}