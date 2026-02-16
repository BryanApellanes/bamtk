namespace Bam.AI.Orchestration.Model
{

    /// <summary>
    /// Concrete implementation of model capabilities.
    /// </summary>
    public class ModelCapabilities : IModelCapabilities
    {
        /// <summary>
        /// Gets the maximum context window size in tokens.
        /// </summary>
        public int MaxContextTokens { get; }


        /// <summary>
        /// Gets the maximum output size in tokens.
        /// </summary>
        public int MaxOutputTokens { get; }


        /// <summary>
        /// Gets whether the model supports tool/function calling.
        /// </summary>
        public bool SupportsToolCalling { get; }


        /// <summary>
        /// Gets whether the model supports vision/image inputs.
        /// </summary>
        public bool SupportsVision { get; }


        /// <summary>
        /// Gets whether the model supports streaming responses.
        /// </summary>
        public bool SupportsStreaming { get; }


        /// <summary>
        /// Gets the languages supported by the model.
        /// </summary>
        public IEnumerable<string> SupportedLanguages { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ModelCapabilities"/> class.
        /// </summary>
        public ModelCapabilities(
            int maxContextTokens,
            int maxOutputTokens,
            bool supportsToolCalling,
            bool supportsVision,
            bool supportsStreaming,
            IEnumerable<string> supportedLanguages)
        {
            MaxContextTokens = maxContextTokens;
            MaxOutputTokens = maxOutputTokens;
            SupportsToolCalling = supportsToolCalling;
            SupportsVision = supportsVision;
            SupportsStreaming = supportsStreaming;
            SupportedLanguages = supportedLanguages ?? Enumerable.Empty<string>();
        }
    }
}