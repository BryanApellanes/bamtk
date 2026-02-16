namespace Bam.AI.Orchestration.Planning
{

    /// <summary>
    /// Concrete implementation of an execution plan.
    /// </summary>
    public class ExecutionPlan : IExecutionPlan
    {
        /// <summary>
        /// Gets the unique plan identifier.
        /// </summary>
        public string PlanId { get; }


        /// <summary>
        /// Gets the execution steps in the plan.
        /// </summary>
        public IEnumerable<IExecutionStep> Steps { get; }


        /// <summary>
        /// Gets the shared context for the plan.
        /// </summary>
        /// <value>
        /// A dictionary for storing data that needs to be shared between steps,
        /// such as intermediate results or configuration.
        /// </value>
        public IDictionary<string, object> SharedContext { get; }


        /// <summary>
        /// Gets the confidence score for this plan.
        /// </summary>
        public double ConfidenceScore { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionPlan"/> class.
        /// </summary>
        public ExecutionPlan(
            string planId,
            IEnumerable<IExecutionStep> steps,
            IDictionary<string, object> sharedContext,
            double confidenceScore)
        {
            PlanId = planId ?? throw new ArgumentNullException(nameof(planId));
            Steps = steps ?? throw new ArgumentNullException(nameof(steps));
            SharedContext = sharedContext ?? new Dictionary<string, object>();
            ConfidenceScore = Math.Clamp(confidenceScore, 0.0, 1.0);
        }
    }
}