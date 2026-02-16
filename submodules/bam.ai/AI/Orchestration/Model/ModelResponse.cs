using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Model
{

    /// <summary>
    /// Concrete implementation of a model response.
    /// </summary>
    public class ModelResponse : IModelResponse
    {
        /// <summary>
        /// Gets the generated content.
        /// </summary>
        public string Content { get; }


        /// <summary>
        /// Gets the tool calls requested by the model.
        /// </summary>
        public IEnumerable<IToolCall> ToolCalls { get; }


        /// <summary>
        /// Gets the reason generation finished.
        /// </summary>
        public FinishReason FinishReason { get; }


        /// <summary>
        /// Gets the token usage statistics.
        /// </summary>
        public IUsageMetrics Usage { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ModelResponse"/> class.
        /// </summary>
        public ModelResponse(
            string content,
            IEnumerable<IToolCall> toolCalls,
            FinishReason finishReason,
            IUsageMetrics usage)
        {
            Content = content ?? string.Empty;
            ToolCalls = toolCalls ?? Enumerable.Empty<IToolCall>();
            FinishReason = finishReason;
            Usage = usage ?? throw new ArgumentNullException(nameof(usage));
        }
    }
}