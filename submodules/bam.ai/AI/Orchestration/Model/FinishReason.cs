namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Why the model stopped generating
    /// </summary>
    public enum FinishReason
    {
        Stop,
        Length,
        ToolCalls,
        ContentFilter,
        Error
    }
}