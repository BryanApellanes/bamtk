using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Factory for creating language model instances based on configuration.
    /// </summary>
    /// <remarks>
    /// This factory allows the application to select which LLM provider to use
    /// at runtime based on configuration. Supported providers include:
    /// - Anthropic (Claude)
    /// - OpenAI (GPT-4, GPT-3.5, etc.)
    /// - Azure OpenAI
    /// - Local models (via Ollama or similar)
    /// - Custom providers
    /// 
    /// The factory uses the strategy pattern to instantiate the appropriate
    /// provider implementation based on configuration settings.
    /// 
    /// Thread Safety: This class is thread-safe. Multiple threads can create
    /// model instances concurrently.
    /// </remarks>
    /// <example>
    /// <code>
    /// // In appsettings.json:
    /// {
    ///   "LanguageModel": {
    ///     "Provider": "Anthropic",
    ///     "ApiKey": "your-api-key",
    ///     "Model": "claude-sonnet-4-20250514",
    ///     "MaxTokens": 4096
    ///   }
    /// }
    /// 
    /// // In code:
    /// var factory = new LanguageModelFactory(configuration, loggerFactory);
    /// var model = factory.CreateLanguageModel();
    /// </code>
    /// </example>
    public class LanguageModelFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;


        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageModelFactory"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="loggerFactory">The logger factory for creating provider loggers.</param>
        public LanguageModelFactory(
            IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }


        /// <summary>
        /// Creates a language model instance based on configuration.
        /// </summary>
        /// <returns>An initialized language model instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the configuration is invalid or the provider is not supported.
        /// </exception>
        /// <remarks>
        /// The factory reads configuration from the "LanguageModel" section and
        /// creates the appropriate provider. Required configuration:
        /// - Provider: The LLM provider name
        /// - ApiKey: The API key (not required for local models)
        /// - Model: The specific model to use
        /// 
        /// Optional configuration:
        /// - MaxTokens: Maximum tokens for responses
        /// - Temperature: Sampling temperature
        /// - Endpoint: Custom API endpoint (for Azure or self-hosted)
        /// - Timeout: Request timeout in seconds
        /// </remarks>
        public ILanguageModel CreateLanguageModel()
        {
            var options = _configuration
                .GetSection("LanguageModel")
                .Get<LanguageModelOptions>();


            if (options == null)
            {
                throw new InvalidOperationException(
                    "LanguageModel configuration section is missing. " +
                    "Please add a LanguageModel section to your appsettings.json");
            }


            if (string.IsNullOrWhiteSpace(options.Provider))
            {
                throw new InvalidOperationException(
                    "LanguageModel.Provider is not configured. " +
                    "Please specify a provider (e.g., 'Anthropic', 'OpenAI', 'AzureOpenAI', 'Local')");
            }


            return options.Provider.ToLowerInvariant() switch
            {
                "anthropic" or "claude" => CreateAnthropicModel(options),
                "openai" or "gpt" => CreateOpenAIModel(options),
                "azureopenai" or "azure" => CreateAzureOpenAIModel(options),
                "local" or "ollama" => CreateLocalModel(options),
                _ => throw new InvalidOperationException(
                    $"Unsupported language model provider: {options.Provider}. " +
                    $"Supported providers: Anthropic, OpenAI, AzureOpenAI, Local")
            };
        }


        /// <summary>
        /// Creates an Anthropic (Claude) language model instance.
        /// </summary>
        private ILanguageModel CreateAnthropicModel(LanguageModelOptions options)
        {
            ValidateApiKey(options, "Anthropic");


            var logger = _loggerFactory.CreateLogger<AnthropicLanguageModel>();

            return new AnthropicLanguageModel(
                apiKey: options.ApiKey,
                model: options.Model ?? "claude-sonnet-4-20250514",
                endpoint: options.Endpoint ?? "https://api.anthropic.com/v1/messages",
                timeout: TimeSpan.FromSeconds(options.TimeoutSeconds ?? 120),
                logger: logger);
        }


        /// <summary>
        /// Creates an OpenAI language model instance.
        /// </summary>
        private ILanguageModel CreateOpenAIModel(LanguageModelOptions options)
        {
            ValidateApiKey(options, "OpenAI");


            var logger = _loggerFactory.CreateLogger<OpenAILanguageModel>();

            return new OpenAILanguageModel(
                apiKey: options.ApiKey,
                model: options.Model ?? "gpt-4",
                endpoint: options.Endpoint ?? "https://api.openai.com/v1/chat/completions",
                timeout: TimeSpan.FromSeconds(options.TimeoutSeconds ?? 120),
                logger: logger);
        }


        /// <summary>
        /// Creates an Azure OpenAI language model instance.
        /// </summary>
        private ILanguageModel CreateAzureOpenAIModel(LanguageModelOptions options)
        {
            ValidateApiKey(options, "AzureOpenAI");


            if (string.IsNullOrWhiteSpace(options.Endpoint))
            {
                throw new InvalidOperationException(
                    "LanguageModel.Endpoint is required for Azure OpenAI. " +
                    "Please specify your Azure OpenAI endpoint URL.");
            }


            var logger = _loggerFactory.CreateLogger<AzureOpenAILanguageModel>();

            return new AzureOpenAILanguageModel(
                apiKey: options.ApiKey,
                model: options.Model ?? "gpt-4",
                endpoint: options.Endpoint,
                deploymentName: options.DeploymentName,
                timeout: TimeSpan.FromSeconds(options.TimeoutSeconds ?? 120),
                logger: logger);
        }


        /// <summary>
        /// Creates a local language model instance (e.g., via Ollama).
        /// </summary>
        private ILanguageModel CreateLocalModel(LanguageModelOptions options)
        {
            var logger = _loggerFactory.CreateLogger<LocalLanguageModel>();

            return new LocalLanguageModel(
                model: options.Model ?? "llama2",
                endpoint: options.Endpoint ?? "http://localhost:11434/api/generate",
                timeout: TimeSpan.FromSeconds(options.TimeoutSeconds ?? 300), // Local models may be slower
                logger: logger);
        }


        /// <summary>
        /// Validates that an API key is provided for cloud providers.
        /// </summary>
        private void ValidateApiKey(LanguageModelOptions options, string providerName)
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException(
                    $"LanguageModel.ApiKey is required for {providerName}. " +
                    $"Please add your API key to the configuration.");
            }
        }
    }

}
