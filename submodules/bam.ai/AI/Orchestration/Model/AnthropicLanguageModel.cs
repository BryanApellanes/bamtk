using Bam.AI.Orchestration.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;


namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Language model implementation for Anthropic's Claude API.
    /// </summary>
    /// <remarks>
    /// This implementation communicates with Anthropic's Messages API to generate
    /// responses using Claude models. It supports:
    /// - Standard message-based interactions
    /// - Tool calling (function calling)
    /// - Streaming responses
    /// - Multi-turn conversations
    /// - System prompts
    /// 
    /// API Documentation: https://docs.anthropic.com/claude/reference/messages_post
    /// 
    /// Thread Safety: This class is thread-safe. Multiple requests can be made
    /// concurrently using the same instance.
    /// 
    /// Error Handling: API errors are wrapped in exceptions with descriptive
    /// messages. Transient errors (rate limits, timeouts) should be retried
    /// by the caller using an appropriate retry policy.
    /// </remarks>
    public class AnthropicLanguageModel : ILanguageModel
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _endpoint;
        private readonly TimeSpan _timeout;
        private readonly ILogger<AnthropicLanguageModel> _logger;
        private readonly HttpClient _httpClient;


        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
        };


        /// <summary>
        /// Gets the capabilities of this language model.
        /// </summary>
        public IModelCapabilities Capabilities { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="AnthropicLanguageModel"/> class.
        /// </summary>
        /// <param name="apiKey">The Anthropic API key.</param>
        /// <param name="model">The model identifier (e.g., "claude-sonnet-4-20250514").</param>
        /// <param name="endpoint">The API endpoint URL.</param>
        /// <param name="timeout">The request timeout.</param>
        /// <param name="logger">Logger instance.</param>
        public AnthropicLanguageModel(
            string apiKey,
            string model,
            string endpoint,
            TimeSpan timeout,
            ILogger<AnthropicLanguageModel> logger)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _timeout = timeout;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));


            _httpClient = new HttpClient
            {
                Timeout = timeout
            };


            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");


            // Set capabilities based on model
            Capabilities = new ModelCapabilities(
                maxContextTokens: GetMaxContextTokens(model),
                maxOutputTokens: GetMaxOutputTokens(model),
                supportsToolCalling: true,
                supportsVision: model.Contains("sonnet") || model.Contains("opus"),
                supportsStreaming: true,
                supportedLanguages: new[] { "en", "es", "fr", "de", "it", "pt", "ja", "ko", "zh" });
        }


        /// <summary>
        /// Generates a response from the language model.
        /// </summary>
        /// <param name="request">The model request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The model's response.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="request"/> is null.
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// Thrown when the API request fails.
        /// </exception>
        /// <remarks>
        /// This method makes a synchronous (non-streaming) request to the Claude API.
        /// The request includes:
        /// - System prompt (if provided)
        /// - Conversation messages
        /// - Available tools (if any)
        /// - Generation parameters (temperature, max_tokens, etc.)
        /// 
        /// Rate Limits: Anthropic enforces rate limits on API requests. Consider
        /// implementing exponential backoff retry logic for production use.
        /// 
        /// Costs: Each request consumes tokens based on input and output length.
        /// Monitor usage through the Anthropic console.
        /// </remarks>
        public async Task<IModelResponse> GenerateAsync(
            IModelRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));


            _logger.LogDebug(
                "Generating response with model {Model} for {MessageCount} messages",
                _model,
                request.Messages.Count());


            var apiRequest = BuildApiRequest(request);
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
                        $"Anthropic API request failed with status {httpResponse.StatusCode}: {responseContent}");
                }


                _logger.LogTrace("API Response: {Response}", responseContent);


                var apiResponse = JsonSerializer.Deserialize<AnthropicApiResponse>(
                    responseContent,
                    JsonOptions);


                return ConvertToModelResponse(apiResponse);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "API request timed out after {Timeout}", _timeout);
                throw new TimeoutException($"API request timed out after {_timeout}", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API request failed");
                throw;
            }
        }


        /// <summary>
        /// Generates a streaming response from the language model.
        /// </summary>
        /// <param name="request">The model request.</param>
        /// <param name="progress">Progress reporter for streaming tokens.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The complete model response after streaming finishes.</returns>
        /// <remarks>
        /// Streaming allows the application to display partial responses as they're
        /// generated, providing a better user experience for long responses.
        /// 
        /// The progress reporter receives individual tokens as they arrive. The final
        /// complete response is returned when generation finishes.
        /// 
        /// Note: This is a simplified implementation. A full production implementation
        /// would parse Server-Sent Events (SSE) from the streaming endpoint.
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


            _logger.LogDebug("Generating streaming response with model {Model}", _model);


            // For simplicity, this implementation falls back to non-streaming
            // A full implementation would:
            // 1. Set "stream": true in the API request
            // 2. Parse Server-Sent Events from the response
            // 3. Report each delta to the progress handler
            // 4. Accumulate the complete response


            _logger.LogWarning(
                "Streaming not fully implemented, using standard generation. " +
                "Implement SSE parsing for true streaming.");


            var response = await GenerateAsync(request, cancellationToken);
            progress.Report(response.Content);


            return response;
        }


        /// <summary>
        /// Builds the API request object from the model request.
        /// </summary>
        private object BuildApiRequest(IModelRequest request)
        {
            var messages = request.Messages.Select(m => new
            {
                role = m.Role.ToString().ToLowerInvariant(),
                content = BuildMessageContent(m)
            }).ToList();


            var apiRequest = new
            {
                model = _model,
                messages,
                max_tokens = request.Parameters?.MaxTokens ?? 4096,
                temperature = request.Parameters?.Temperature ?? 0.7,
                top_p = request.Parameters?.TopP,
                stop_sequences = request.Parameters?.StopSequences,
                system = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : request.SystemPrompt,
                tools = request.AvailableTools?.Any() == true ? BuildTools(request.AvailableTools) : null
            };


            return apiRequest;
        }


        /// <summary>
        /// Builds message content, handling both text and tool calls.
        /// </summary>
        private object BuildMessageContent(IMessage message)
        {
            // Simple text message
            if (!message.ToolCalls?.Any() == true && !message.ToolResults?.Any() == true)
            {
                return message.Content;
            }


            // Message with tool calls or results
            var contentBlocks = new List<object>();


            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                contentBlocks.Add(new
                {
                    type = "text",
                    text = message.Content
                });
            }


            if (message.ToolCalls?.Any() == true)
            {
                foreach (var toolCall in message.ToolCalls)
                {
                    contentBlocks.Add(new
                    {
                        type = "tool_use",
                        id = Guid.NewGuid().ToString(),
                        name = toolCall.ToolName,
                        input = toolCall.Parameters
                    });
                }
            }


            if (message.ToolResults?.Any() == true)
            {
                foreach (var result in message.ToolResults)
                {
                    contentBlocks.Add(new
                    {
                        type = "tool_result",
                        tool_use_id = result.ToolId,
                        content = result.Success ? result.Data?.ToString() : result.ErrorMessage
                    });
                }
            }


            return contentBlocks;
        }


        /// <summary>
        /// Builds the tools array for the API request.
        /// </summary>
        private object[] BuildTools(IEnumerable<IToolDescriptor> tools)
        {
            return tools.Select(tool => new
            {
                name = tool.Id,
                description = tool.Description,
                input_schema = JsonSerializer.Deserialize<object>(tool.InputSchema.SchemaDefinition)
            }).ToArray();
        }


        /// <summary>
        /// Converts the API response to a model response.
        /// </summary>
        private IModelResponse ConvertToModelResponse(AnthropicApiResponse apiResponse)
        {
            var content = ExtractTextContent(apiResponse.Content);
            var toolCalls = ExtractToolCalls(apiResponse.Content);
            var finishReason = ConvertFinishReason(apiResponse.StopReason);


            var usage = new UsageMetrics(
                promptTokens: apiResponse.Usage.InputTokens,
                completionTokens: apiResponse.Usage.OutputTokens,
                totalTokens: apiResponse.Usage.InputTokens + apiResponse.Usage.OutputTokens);


            return new ModelResponse(
                content: content,
                toolCalls: toolCalls,
                finishReason: finishReason,
                usage: usage);
        }


        /// <summary>
        /// Extracts text content from the API response content blocks.
        /// </summary>
        private string ExtractTextContent(List<AnthropicContentBlock> contentBlocks)
        {
            if (contentBlocks == null || !contentBlocks.Any())
                return string.Empty;


            var textBlocks = contentBlocks
                .Where(b => b.Type == "text")
                .Select(b => b.Text)
                .Where(t => !string.IsNullOrEmpty(t));


            return string.Join("\n", textBlocks);
        }


        /// <summary>
        /// Extracts tool calls from the API response content blocks.
        /// </summary>
        private List<IToolCall> ExtractToolCalls(List<AnthropicContentBlock> contentBlocks)
        {
            if (contentBlocks == null || !contentBlocks.Any())
                return new List<IToolCall>();


            return contentBlocks
                .Where(b => b.Type == "tool_use")
                .Select(b => new ToolCall(
                    toolId: b.Id,
                    toolName: b.Name,
                    parameters: b.Input as IDictionary<string, object> ?? new Dictionary<string, object>(),
                    result: null)) // Result will be filled in during execution
                .Cast<IToolCall>()
                .ToList();
        }


        /// <summary>
        /// Converts Anthropic's stop reason to our finish reason enum.
        /// </summary>
        private FinishReason ConvertFinishReason(string stopReason)
        {
            return stopReason switch
            {
                "end_turn" => FinishReason.Stop,
                "max_tokens" => FinishReason.Length,
                "tool_use" => FinishReason.ToolCalls,
                "stop_sequence" => FinishReason.Stop,
                _ => FinishReason.Stop
            };
        }


        /// <summary>
        /// Gets the maximum context tokens for a model.
        /// </summary>
        private int GetMaxContextTokens(string model)
        {
            // Claude models generally support 200k tokens
            return model.ToLowerInvariant() switch
            {
                var m when m.Contains("opus") => 200000,
                var m when m.Contains("sonnet") => 200000,
                var m when m.Contains("haiku") => 200000,
                _ => 100000 // Conservative default
            };
        }


        /// <summary>
        /// Gets the maximum output tokens for a model.
        /// </summary>
        private int GetMaxOutputTokens(string model)
        {
            // Claude models support 4k output tokens by default
            return 4096;
        }


        // API Response DTOs
        private class AnthropicApiResponse
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string Role { get; set; }
            public List<AnthropicContentBlock> Content { get; set; }
            public string Model { get; set; }
            public string StopReason { get; set; }
            public string StopSequence { get; set; }
            public AnthropicUsage Usage { get; set; }
        }


        private class AnthropicContentBlock
        {
            public string Type { get; set; }
            public string Text { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public object Input { get; set; }
        }


        private class AnthropicUsage
        {
            public int InputTokens { get; set; }
            public int OutputTokens { get; set; }
        }
    }
}
