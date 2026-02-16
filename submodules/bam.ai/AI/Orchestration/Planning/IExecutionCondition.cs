namespace Bam.AI.Orchestration.Planning
{
    /// <summary>
    /// Condition for executing a step
    /// </summary>
    public interface IExecutionCondition
    {
        string Expression { get; }
        ConditionType Type { get; }
    }
}