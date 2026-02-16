namespace Bam.AI.Orchestration.Model
{

    /// <summary>
    /// Configuration options for language models.
    /// </summary>
    /// <remarks>
    /// These options are typically bound from the "LanguageModel" section
    /// of appsettings.json. The options support all major LLM providers
    /// with provider-specific settings.
    /// </remarks>
    public class LanguageModelOptions
    {
        /// <summary>
        /// Gets or sets the LLM provider name.
        /// </summary>
        /// <value>
        /// Valid values: "Anthropic", "OpenAI", "AzureOpenAI", "Local".
        /// Case-insensitive.
        /// </value>
        public string Provider { get; set; }


        /// <summary>
        /// Gets or sets the API key for authentication.
        /// </summary>
        /// <value>
        /// Required for cloud providers (Anthropic, OpenAI, Azure).
        /// Not required for local models.
        /// </value>
        public string ApiKey { get; set; }


        /// <summary>
        /// Gets or sets the model identifier.
        /// </summary>
        /// <value>
        /// Examples:
        /// - Anthropic: "claude-sonnet-4-20250514", "claude-opus-4-20250514"
        /// - OpenAI: "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo"
        /// - Local: "llama2", "mistral", "codellama"
        /// </value>
        public string Model { get; set; }


        /// <summary>
        /// Gets or sets the API endpoint URL.
        /// </summary>
        /// <value>
        /// Optional for standard providers (uses default endpoints).
        /// Required for Azure OpenAI (your deployment endpoint).
        /// Optional for local models (defaults to localhost:11434).
        /// </value>
        public string Endpoint { get; set; }


        /// <summary>
        /// Gets or sets the deployment name (Azure OpenAI only).
        /// </summary>
        public string DeploymentName { get; set; }


        /// <summary>
        /// Gets or sets the maximum number of tokens in responses.
        /// </summary>
        /// <value>
        /// Defaults to model-specific limits if not specified.
        /// Common values: 1024, 2048, 4096, 8192.
        /// </value>
        public int? MaxTokens { get; set; }


        /// <summary>
        /// Gets or sets the sampling temperature.
        /// </summary>
        /// <value>
        /// Range: 0.0 to 1.0 (or higher for some models).
        /// - 0.0: Deterministic, focused responses
        /// - 1.0: Creative, diverse responses
        /// Default is typically 0.7.
        /// </value>
        public double? Temperature { get; set; }


        /// <summary>
        /// Gets or sets the request timeout in seconds.
        /// </summary>
        /// <value>
        /// Default: 120 seconds for cloud providers, 300 for local models.
        /// Increase for long-running generations.
        /// </value>
        public int? TimeoutSeconds { get; set; }


        /// <summary>
        /// Gets or sets whether to use streaming responses.
        /// </summary>
        /// <value>
        /// When true, responses are streamed token-by-token.
        /// Useful for real-time UI updates.
        /// </value>
        public bool? EnableStreaming { get; set; }


        /// <summary>
        /// Gets or sets additional provider-specific settings.
        /// </summary>
        /// <value>
        /// A dictionary for provider-specific configuration that doesn't
        /// fit the common schema. For example:
        /// - OpenAI: "organization_id"
        /// - Anthropic: "anthropic_version"
        /// </value>
        public Dictionary<string, string> AdditionalSettings { get; set; }
    }
}