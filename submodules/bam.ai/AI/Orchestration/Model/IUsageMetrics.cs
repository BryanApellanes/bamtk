namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Token usage metrics
    /// </summary>
    public interface IUsageMetrics
    {
        int PromptTokens { get; }
        int CompletionTokens { get; }
        int TotalTokens { get; }
    }
}