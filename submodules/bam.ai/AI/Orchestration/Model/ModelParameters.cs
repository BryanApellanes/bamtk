namespace Bam.AI.Orchestration.Model
{

    /// <summary>
    /// Concrete implementation of model generation parameters.
    /// </summary>
    /// <remarks>
    /// These parameters control how the model generates responses. Different
    /// providers may support different parameters, and some parameters may be
    /// ignored by certain models.
    /// </remarks>
    public class ModelParameters : IModelParameters
    {
        /// <summary>
        /// Gets the sampling temperature.
        /// </summary>
        /// <value>
        /// Controls randomness in generation. Range: 0.0 to 2.0 (some models).
        /// - 0.0: Deterministic, always picks highest probability token
        /// - 0.7: Balanced (default)
        /// - 1.0: More creative
        /// - 2.0: Very random (use with caution)
        /// </value>
        public double Temperature { get; }


        /// <summary>
        /// Gets the maximum number of tokens to generate.
        /// </summary>
        /// <value>
        /// The maximum length of the generated response. The actual response
        /// may be shorter if the model finishes naturally or hits a stop sequence.
        /// </value>
        public int MaxTokens { get; }


        /// <summary>
        /// Gets the nucleus sampling parameter.
        /// </summary>
        /// <value>
        /// An alternative to temperature for controlling randomness. Range: 0.0 to 1.0.
        /// The model considers only tokens with cumulative probability up to top_p.
        /// - 0.1: Very focused (top 10% of probability mass)
        /// - 0.9: More diverse (top 90% of probability mass)
        /// - 1.0: All tokens considered
        /// </value>
        public double TopP { get; }


        /// <summary>
        /// Gets the frequency penalty.
        /// </summary>
        /// <value>
        /// Reduces repetition by penalizing tokens based on their frequency.
        /// Range: -2.0 to 2.0.
        /// - Positive: Discourage repetition
        /// - Negative: Encourage repetition
        /// - 0.0: No penalty
        /// </value>
        public double FrequencyPenalty { get; }


        /// <summary>
        /// Gets the presence penalty.
        /// </summary>
        /// <value>
        /// Encourages topic diversity by penalizing tokens that have appeared.
        /// Range: -2.0 to 2.0.
        /// - Positive: Encourage new topics
        /// - Negative: Stay on topic
        /// - 0.0: No penalty
        /// </value>
        public double PresencePenalty { get; }


        /// <summary>
        /// Gets the stop sequences.
        /// </summary>
        /// <value>
        /// Strings that, when generated, will cause generation to stop.
        /// Useful for controlling output format or length.
        /// Example: ["\n\n", "END", "###"]
        /// </value>
        public IEnumerable<string> StopSequences { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ModelParameters"/> class with defaults.
        /// </summary>
        public ModelParameters()
            : this(
                temperature: 0.7,
                maxTokens: 4096,
                topP: 1.0,
                frequencyPenalty: 0.0,
                presencePenalty: 0.0,
                stopSequences: null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ModelParameters"/> class.
        /// </summary>
        /// <param name="temperature">The temperature.</param>
        /// <param name="maxTokens">The maximum tokens.</param>
        /// <param name="topP">The top-p value.</param>
        /// <param name="frequencyPenalty">The frequency penalty.</param>
        /// <param name="presencePenalty">The presence penalty.</param>
        /// <param name="stopSequences">The stop sequences.</param>
        public ModelParameters(
            double temperature,
            int maxTokens,
            double topP,
            double frequencyPenalty,
            double presencePenalty,
            IEnumerable<string> stopSequences)
        {
            Temperature = Math.Clamp(temperature, 0.0, 2.0);
            MaxTokens = Math.Max(1, maxTokens);
            TopP = Math.Clamp(topP, 0.0, 1.0);
            FrequencyPenalty = Math.Clamp(frequencyPenalty, -2.0, 2.0);
            PresencePenalty = Math.Clamp(presencePenalty, -2.0, 2.0);
            StopSequences = stopSequences ?? Enumerable.Empty<string>();
        }
    }
}