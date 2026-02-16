namespace Bam.AI.Orchestration.Planning
{

    /// <summary>
    /// Concrete implementation of an execution condition.
    /// </summary>
    public class ExecutionCondition : IExecutionCondition
    {
        /// <summary>
        /// Gets the condition expression.
        /// </summary>
        /// <value>
        /// A string expression that can be evaluated to determine if the step
        /// should execute. For simple conditions, this might be "true" or
        /// "previous_step.success == true". For complex conditions, this could
        /// be a more sophisticated expression language.
        /// </value>
        public string Expression { get; }


        /// <summary>
        /// Gets the condition type.
        /// </summary>
        public ConditionType Type { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionCondition"/> class.
        /// </summary>
        public ExecutionCondition(string expression, ConditionType type)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Type = type;
        }
    }
}