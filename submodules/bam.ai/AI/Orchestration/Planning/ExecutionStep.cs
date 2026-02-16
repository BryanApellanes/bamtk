using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Planning
{

    /// <summary>
    /// Concrete implementation of an execution step.
    /// </summary>
    public class ExecutionStep : IExecutionStep
    {
        /// <summary>
        /// Gets the execution order.
        /// </summary>
        public int Order { get; }


        /// <summary>
        /// Gets the step description.
        /// </summary>
        public string Description { get; }


        /// <summary>
        /// Gets the tool to execute.
        /// </summary>
        public ISelectedTool Tool { get; }


        /// <summary>
        /// Gets the IDs of steps this step depends on.
        /// </summary>
        public IEnumerable<string> DependsOn { get; }


        /// <summary>
        /// Gets the execution condition.
        /// </summary>
        public IExecutionCondition Condition { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionStep"/> class.
        /// </summary>
        public ExecutionStep(
            int order,
            string description,
            ISelectedTool tool,
            IEnumerable<string> dependsOn,
            IExecutionCondition condition)
        {
            Order = order;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Tool = tool ?? throw new ArgumentNullException(nameof(tool));
            DependsOn = dependsOn ?? Enumerable.Empty<string>();
            Condition = condition ?? new ExecutionCondition("true", ConditionType.Always);
        }
    }
}