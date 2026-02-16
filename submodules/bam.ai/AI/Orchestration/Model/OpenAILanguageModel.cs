using Bam.AI.Orchestration.Tools;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Language model implementation for OpenAI's GPT models.
    /// </summary>
    /// <remarks>
    /// This implementation communicates with OpenAI's Chat Completions API.
    /// Supports GPT-4, GPT-4 Turbo, GPT-3.5 Turbo, and other chat models.
    /// 
    /// API Documentation: https://platform.openai.com/docs/api-reference/chat
    /// 
    /// Key differences from Anthropic:
    /// - Uses "messages" array with "assistant" role for tool calls
    /// - Tool calling format is slightly different (functions vs tools)
    /// - Different token limits per model
    /// - Different pricing structure
    /// 
    /// Thread Safety: This class is thread-safe.
    /// </remarks>
    public class OpenAILanguageModel : ILanguageModel
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _endpoint;
        private readonly TimeSpan _timeout;
        private readonly ILogger<OpenAILanguageModel> _logger;
        private readonly HttpClient _httpClient;


        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };


        public IModelCapabilities Capabilities { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAILanguageModel"/> class.
        /// </summary>
        public OpenAILanguageModel(
            string apiKey,
            string model,
            string endpoint,
            TimeSpan timeout,
            ILogger<OpenAILanguageModel> logger)
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


            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");


            Capabilities = new ModelCapabilities(
                maxContextTokens: GetMaxContextTokens(model),
                maxOutputTokens: GetMaxOutputTokens(model),
                supportsToolCalling: true,
                supportsVision: model.Contains("gpt-4") && model.Contains("vision"),
                supportsStreaming: true,
                supportedLanguages: new[] { "en", "es", "fr", "de", "it", "pt", "ja", "ko", "zh", "ru", "ar" });
        }


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
                        $"OpenAI API request failed with status {httpResponse.StatusCode}: {responseContent}");
                }


                _logger.LogTrace("API Response: {Response}", responseContent);


                var apiResponse = JsonSerializer.Deserialize<OpenAIApiResponse>(
                    responseContent,
                    JsonOptions);


                return ConvertToModelResponse(apiResponse);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "API request timed out after {Timeout}", _timeout);
                throw new TimeoutException($"API request timed out after {_timeout}", ex);
            }
        }


        public async Task<IModelResponse> GenerateStreamingAsync(
            IModelRequest request,
            IProgress<string> progress,
            CancellationToken cancellationToken = default)
        {
            // Simplified implementation - production code would parse SSE stream
            _logger.LogWarning("Streaming not fully implemented, using standard generation");
            var response = await GenerateAsync(request, cancellationToken);
            progress.Report(response.Content);
            return response;
        }


        private object BuildApiRequest(IModelRequest request)
        {
            var messages = new List<object>();


            // Add system message if present
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(new
                {
                    role = "system",
                    content = request.SystemPrompt
                });
            }


            // Add conversation messages
            messages.AddRange(request.Messages.Select(m => new
            {
                role = m.Role.ToString().ToLowerInvariant(),
                content = m.Content,
                tool_calls = m.ToolCalls?.Any() == true ? BuildToolCalls(m.ToolCalls) : null,
                tool_call_id = m.ToolResults?.Any() == true ? m.ToolResults.First().ToolId : null
            }));


            return new
            {
                model = _model,
                messages,
                max_tokens = request.Parameters?.MaxTokens ?? 4096,
                temperature = request.Parameters?.Temperature ?? 0.7,
                top_p = request.Parameters?.TopP,
                frequency_penalty = request.Parameters?.FrequencyPenalty,
                presence_penalty = request.Parameters?.PresencePenalty,
                stop = request.Parameters?.StopSequences,
                tools = request.AvailableTools?.Any() == true ? BuildTools(request.AvailableTools) : null
            };
        }


        private object[] BuildToolCalls(IEnumerable<IToolCall> toolCalls)
        {
            return toolCalls.Select(tc => new
            {
                id = tc.ToolId,
                type = "function",
                function = new
                {
                    name = tc.ToolName,
                    arguments = JsonSerializer.Serialize(tc.Parameters)
                }
            }).ToArray();
        }


        private object[] BuildTools(IEnumerable<IToolDescriptor> tools)
        {
            return tools.Select(tool => new
            {
                type = "function",
                function = new
                {
                    name = tool.Id,
                    description = tool.Description,
                    parameters = JsonSerializer.Deserialize<object>(tool.InputSchema.SchemaDefinition)
                }
            }).ToArray();
        }


        private IModelResponse ConvertToModelResponse(OpenAIApiResponse apiResponse)
        {
            var choice = apiResponse.Choices?.FirstOrDefault();
            if (choice == null)
            {
                throw new InvalidOperationException("API response contained no choices");
            }


            var content = choice.Message.Content ?? string.Empty;
            var toolCalls = ExtractToolCalls(choice.Message.ToolCalls);
            var finishReason = ConvertFinishReason(choice.FinishReason);


            var usage = new UsageMetrics(
                promptTokens: apiResponse.Usage.PromptTokens,
                completionTokens: apiResponse.Usage.CompletionTokens,
                totalTokens: apiResponse.Usage.TotalTokens);


            return new ModelResponse(content, toolCalls, finishReason, usage);
        }


        private List<IToolCall> ExtractToolCalls(List<OpenAIToolCall> toolCalls)
        {
            if (toolCalls == null || !toolCalls.Any())
                return new List<IToolCall>();


            return toolCalls.Select(tc =>
            {
                var parameters = string.IsNullOrWhiteSpace(tc.Function.Arguments)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(tc.Function.Arguments);


                return new ToolCall(
                    toolId: tc.Id,
                    toolName: tc.Function.Name,
                    parameters: parameters,
                    result: null);
            }).Cast<IToolCall>().ToList();
        }


        private FinishReason ConvertFinishReason(string finishReason)
        {
            return finishReason switch
            {
                "stop" => FinishReason.Stop,
                "length" => FinishReason.Length,
                "tool_calls" or "function_call" => FinishReason.ToolCalls,
                "content_filter" => FinishReason.ContentFilter,
                _ => FinishReason.Stop
            };
        }


        private int GetMaxContextTokens(string model)
        {
            return model.ToLowerInvariant() switch
            {
                var m when m.Contains("gpt-4-turbo") => 128000,
                var m when m.Contains("gpt-4") => 8192,
                var m when m.Contains("gpt-3.5-turbo-16k") => 16384,
                var m when m.Contains("gpt-3.5") => 4096,
                _ => 4096
            };
        }


        private int GetMaxOutputTokens(string model)
        {
            return model.ToLowerInvariant() switch
            {
                var m when m.Contains("gpt-4-turbo") => 4096,
                var m when m.Contains("gpt-4") => 4096,
                var m when m.Contains("gpt-3.5") => 4096,
                _ => 4096
            };
        }


        // API Response DTOs
        private class OpenAIApiResponse
        {
            public string Id { get; set; }
            public string Object { get; set; }
            public long Created { get; set; }
            public string Model { get; set; }
            public List<OpenAIChoice> Choices { get; set; }
            public OpenAIUsage Usage { get; set; }
        }


        private class OpenAIChoice
        {
            public int Index { get; set; }
            public OpenAIMessage Message { get; set; }
            public string FinishReason { get; set; }
        }


        private class OpenAIMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
            public List<OpenAIToolCall> ToolCalls { get; set; }
        }


        private class OpenAIToolCall
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public OpenAIFunction Function { get; set; }
        }


        private class OpenAIFunction
        {
            public string Name { get; set; }
            public string Arguments { get; set; }
        }


        private class OpenAIUsage
        {
            public int PromptTokens { get; set; }
            public int CompletionTokens { get; set; }
            public int TotalTokens { get; set; }
        }
    }
}


