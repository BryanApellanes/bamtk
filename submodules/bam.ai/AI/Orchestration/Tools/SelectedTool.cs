namespace Bam.AI.Orchestration.Tools
{

    /// <summary>
    /// Concrete implementation of a selected tool.
    /// </summary>
    /// <remarks>
    /// Represents a tool that has been selected for execution, along with
    /// its parameters, execution order, and confidence score.
    /// </remarks>
    public class SelectedTool : ISelectedTool
    {
        /// <summary>
        /// Gets the tool descriptor.
        /// </summary>
        public IToolDescriptor Descriptor { get; }


        /// <summary>
        /// Gets the parameters to pass to the tool.
        /// </summary>
        /// <value>
        /// A dictionary mapping parameter names to their values. May contain
        /// null values for required parameters that couldn't be extracted -
        /// these should be handled by the tool executor or LLM.
        /// </value>
        public IDictionary<string, object> Parameters { get; }


        /// <summary>
        /// Gets the execution order.
        /// </summary>
        /// <value>
        /// A zero-based index indicating when this tool should execute relative
        /// to other selected tools. Tools with lower order values execute first.
        /// </value>
        public int ExecutionOrder { get; }


        /// <summary>
        /// Gets the confidence score for this selection.
        /// </summary>
        /// <value>
        /// A value between 0.0 and 1.0 indicating how confident the selector
        /// is that this tool is appropriate for the task.
        /// </value>
        public double ConfidenceScore { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedTool"/> class.
        /// </summary>
        public SelectedTool(
            IToolDescriptor descriptor,
            IDictionary<string, object> parameters,
            int executionOrder,
            double confidenceScore)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Parameters = parameters ?? new Dictionary<string, object>();
            ExecutionOrder = executionOrder;
            ConfidenceScore = Math.Clamp(confidenceScore, 0.0, 1.0);
        }
    }
}