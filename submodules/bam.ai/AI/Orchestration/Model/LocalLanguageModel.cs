using Bam.AI.Orchestration.Tools;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Language model implementation for local models via Ollama or similar services.
    /// </summary>
    /// <remarks>
    /// This implementation supports running open-source LLMs locally using Ollama,
    /// LM Studio, or other compatible local inference servers. Benefits include:
    /// - No API costs
    /// - Complete data privacy (no external API calls)
    /// - Customizable models and parameters
    /// - Offline operation
    /// 
    /// Supported local inference servers:
    /// - Ollama (recommended): https://ollama.ai
    /// - LM Studio: https://lmstudio.ai
    /// - LocalAI: https://localai.io
    /// - Text Generation WebUI (oobabooga)
    /// 
    /// Common models available:
    /// - Llama 2, Llama 3
    /// - Mistral, Mixtral
    /// - CodeLlama
    /// - Phi-2, Phi-3
    /// - Gemma
    /// 
    /// Trade-offs:
    /// - Generally slower than cloud APIs
    /// - Lower quality than frontier models (GPT-4, Claude)
    /// - Requires GPU for reasonable performance
    /// - Limited context windows (typically 4k-8k tokens)
    /// 
    /// Thread Safety: This class is thread-safe.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Install Ollama and pull a model:
    /// // $ ollama pull llama2
    /// 
    /// // appsettings.json:
    /// {
    ///   "LanguageModel": {
    ///     "Provider": "Local",
    ///     "Model": "llama2",
    ///     "Endpoint": "http://localhost:11434/api/generate"
    ///   }
    /// }
    /// </code>
    /// </example>
    public class LocalLanguageModel : ILanguageModel
    {
        private readonly string _model;
        private readonly string _endpoint;
        private readonly TimeSpan _timeout;
        private readonly ILogger<LocalLanguageModel> _logger;
        private readonly HttpClient _httpClient;


        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };


        /// <summary>
        /// Gets the capabilities of this language model.
        /// </summary>
        public IModelCapabilities Capabilities { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="LocalLanguageModel"/> class.
        /// </summary>
        /// <param name="model">The model name (e.g., "llama2", "mistral", "codellama").</param>
        /// <param name="endpoint">The local inference server endpoint.</param>
        /// <param name="timeout">The request timeout.</param>
        /// <param name="logger">Logger instance.</param>
        /// <remarks>
        /// Default endpoint for Ollama is http://localhost:11434/api/generate
        /// 
        /// Make sure the model is pulled/downloaded before use:
        /// $ ollama pull llama2
        /// 
        /// For other inference servers, consult their documentation for the
        /// correct endpoint format.
        /// </remarks>
        public LocalLanguageModel(
            string model,
            string endpoint,
            TimeSpan timeout,
            ILogger<LocalLanguageModel> logger)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _timeout = timeout;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));


            _httpClient = new HttpClient
            {
                Timeout = timeout
            };


            // Local models generally have more limited capabilities
            Capabilities = new ModelCapabilities(
                maxContextTokens: GetMaxContextTokens(model),
                maxOutputTokens: GetMaxOutputTokens(model),
                supportsToolCalling: false, // Most local models don't support tool calling yet
                supportsVision: model.Contains("vision") || model.Contains("llava"),
                supportsStreaming: true,
                supportedLanguages: new[] { "en" }); // Most optimized for English


            _logger.LogInformation(
                "Initialized local model {Model} with endpoint {Endpoint}",
                _model,
                _endpoint);
        }


        /// <summary>
        /// Generates a response from the local language model.
        /// </summary>
        /// <param name="request">The model request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The model's response.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="request"/> is null.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// Thrown when the API request fails or the server is not running.
        /// </exception>
        /// <remarks>
        /// Local model generation can be significantly slower than cloud APIs,
        /// especially without GPU acceleration:
        /// - CPU-only: 1-5 tokens/second
        /// - GPU (consumer): 10-50 tokens/second
        /// - GPU (high-end): 50-200 tokens/second
        /// 
        /// Consider using streaming for better user experience with slower models.
        /// 
        /// Common errors:
        /// - Connection refused: Ollama/server not running
        /// - Model not found: Model not pulled/downloaded
        /// - Out of memory: Model too large for available VRAM
        /// </remarks>
        public async Task<IModelResponse> GenerateAsync(
            IModelRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));


            _logger.LogDebug(
                "Generating response with local model {Model} for {MessageCount} messages",
                _model,
                request.Messages.Count());


            // Build prompt from messages (local models typically use a single prompt string)
            var prompt = BuildPrompt(request);


            var apiRequest = new
            {
                model = _model,
                prompt,
                stream = false,
                options = new
                {
                    temperature = request.Parameters?.Temperature ?? 0.7,
                    num_predict = request.Parameters?.MaxTokens ?? 2048,
                    top_p = request.Parameters?.TopP,
                    stop = request.Parameters?.StopSequences
                }
            };


            var jsonRequest = JsonSerializer.Serialize(apiRequest, JsonOptions);


            _logger.LogTrace("API Request: {Request}", jsonRequest);


            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };


            try
            {
                var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
                var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);


                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "API request failed with status {StatusCode}: {Response}",
                        httpResponse.StatusCode,
                        responseContent);


                    throw new HttpRequestException(
                        $"Local model API request failed with status {httpResponse.StatusCode}: {responseContent}");
                }


                _logger.LogTrace("API Response: {Response}", responseContent);


                var apiResponse = JsonSerializer.Deserialize<LocalModelApiResponse>(
                    responseContent,
                    JsonOptions);


                return ConvertToModelResponse(apiResponse);
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("Connection refused"))
            {
                _logger.LogError(
                    "Cannot connect to local model server at {Endpoint}. " +
                    "Make sure Ollama or your inference server is running.",
                    _endpoint);
                throw new InvalidOperationException(
                    $"Local model server not running at {_endpoint}. " +
                    "Please start Ollama with 'ollama serve' or start your inference server.",
                    ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "API request timed out after {Timeout}", _timeout);
                throw new TimeoutException($"API request timed out after {_timeout}", ex);
            }
        }


        /// <summary>
        /// Generates a streaming response from the local language model.
        /// </summary>
        /// <param name="request">The model request.</param>
        /// <param name="progress">Progress reporter for streaming tokens.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The complete model response after streaming finishes.</returns>
        /// <remarks>
        /// Streaming is particularly useful for local models since they can be slow.
        /// This allows the UI to show partial results as they're generated.
        /// 
        /// This simplified implementation falls back to non-streaming.
        /// A full implementation would:
        /// 1. Set stream=true in the request
        /// 2. Read the response stream line by line
        /// 3. Parse each JSON chunk
        /// 4. Report tokens via progress
        /// 5. Accumulate the complete response
        /// </remarks>
        public async Task<IModelResponse> GenerateStreamingAsync(
            IModelRequest request,
            IProgress<string> progress,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (progress == null)
                throw new ArgumentNullException(nameof(progress));


            _logger.LogWarning("Streaming not fully implemented, using standard generation");
            var response = await GenerateAsync(request, cancellationToken);
            progress.Report(response.Content);
            return response;
        }


        /// <summary>
        /// Builds a single prompt string from the request messages.
        /// </summary>
        /// <param name="request">The model request.</param>
        /// <returns>A formatted prompt string.</returns>
        /// <remarks>
        /// Most local models don't have a native message API like OpenAI/Anthropic.
        /// Instead, they expect a single prompt string with a specific format.
        /// 
        /// This implementation uses a common format:
        /// System: {system prompt}
        /// User: {user message}
        /// Assistant: {assistant message}
        /// User: {next user message}
        /// Assistant:
        /// 
        /// Some models have specific prompt formats (Llama 2, Mistral, etc.).
        /// For production use, implement model-specific prompt templates.
        /// </remarks>
        private string BuildPrompt(IModelRequest request)
        {
            var promptBuilder = new StringBuilder();


            // Add system prompt
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                promptBuilder.AppendLine($"System: {request.SystemPrompt}");
                promptBuilder.AppendLine();
            }


            // Add conversation messages
            foreach (var message in request.Messages)
            {
                var role = message.Role switch
                {
                    MessageRole.User => "User",
                    MessageRole.Assistant => "Assistant",
                    MessageRole.System => "System",
                    _ => "User"
                };


                promptBuilder.AppendLine($"{role}: {message.Content}");
            }


            // Add prompt for assistant response
            promptBuilder.Append("Assistant:");


            return promptBuilder.ToString();
        }


        /// <summary>
        /// Converts the local model API response to our model response format.
        /// </summary>
        private IModelResponse ConvertToModelResponse(LocalModelApiResponse apiResponse)
        {
            var content = apiResponse.Response ?? string.Empty;


            // Local models typically don't provide detailed finish reasons
            var finishReason = apiResponse.Done ? FinishReason.Stop : FinishReason.Length;


            // Estimate token usage if not provided
            var promptTokens = EstimateTokens(apiResponse.Context ?? string.Empty);
            var completionTokens = EstimateTokens(content);


            var usage = new UsageMetrics(
                promptTokens: promptTokens,
                completionTokens: completionTokens,
                totalTokens: promptTokens + completionTokens);


            return new ModelResponse(
                content: content,
                toolCalls: new List<IToolCall>(), // Local models don't support tool calling yet
                finishReason: finishReason,
                usage: usage);
        }


        /// <summary>
        /// Estimates token count for a string.
        /// </summary>
        /// <param name="text">The text to estimate.</param>
        /// <returns>Estimated token count.</returns>
        /// <remarks>
        /// This is a rough approximation. For accurate counts, use a proper tokenizer
        /// for the specific model (e.g., sentencepiece for Llama models).
        /// 
        /// Rule of thumb: ~4 characters per token for English text.
        /// </remarks>
        private int EstimateTokens(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;


            // Rough approximation: 1 token ≈ 4 characters
            return text.Length / 4;
        }


        /// <summary>
        /// Gets the maximum context tokens for a model.
        /// </summary>
        private int GetMaxContextTokens(string model)
        {
            return model.ToLowerInvariant() switch
            {
                var m when m.Contains("llama3") => 8192,
                var m when m.Contains("llama2") => 4096,
                var m when m.Contains("mistral") => 8192,
                var m when m.Contains("mixtral") => 32768,
                var m when m.Contains("codellama") => 16384,
                var m when m.Contains("phi-3") => 4096,
                _ => 4096 // Conservative default
            };
        }


        /// <summary>
        /// Gets the maximum output tokens for a model.
        /// </summary>
        private int GetMaxOutputTokens(string model)
        {
            // Most local models can generate up to their context window
            return GetMaxContextTokens(model);
        }


        // API Response DTOs for Ollama
        private class LocalModelApiResponse
        {
            public string Model { get; set; }
            public string Response { get; set; }
            public bool Done { get; set; }
            public string Context { get; set; }
            public long TotalDuration { get; set; }
            public long LoadDuration { get; set; }
            public int PromptEvalCount { get; set; }
            public int EvalCount { get; set; }
        }
    }
}




