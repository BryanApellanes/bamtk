namespace Bam.AI.Orchestration.Core
{
    /// <summary>
    /// Status of an agent response
    /// </summary>
    public enum ResponseStatus
    {
        Success,
        PartialSuccess,
        Failed,
        RequiresUserInput
    }
}