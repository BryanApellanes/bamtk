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
    /// Language model implementation for Azure OpenAI Service.
    /// </summary>
    /// <remarks>
    /// Azure OpenAI provides the same models as OpenAI but with additional features:
    /// - Enterprise-grade security and compliance
    /// - Regional deployment options
    /// - Virtual network support
    /// - Managed identity authentication
    /// - Content filtering
    /// 
    /// Key differences from standard OpenAI:
    /// - Uses Azure-specific endpoints (your-resource.openai.azure.com)
    /// - Requires deployment name instead of just model name
    /// - Uses api-key header instead of Bearer token (by default)
    /// - Different API version scheme
    /// 
    /// API Documentation: https://learn.microsoft.com/en-us/azure/ai-services/openai/reference
    /// 
    /// Thread Safety: This class is thread-safe.
    /// </remarks>
    /// <example>
    /// <code>
    /// // appsettings.json:
    /// {
    ///   "LanguageModel": {
    ///     "Provider": "AzureOpenAI",
    ///     "ApiKey": "your-azure-key",
    ///     "Endpoint": "https://your-resource.openai.azure.com",
    ///     "DeploymentName": "gpt-4-deployment",
    ///     "Model": "gpt-4"
    ///   }
    /// }
    /// </code>
    /// </example>
    public class AzureOpenAILanguageModel : ILanguageModel
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _endpoint;
        private readonly string _deploymentName;
        private readonly TimeSpan _timeout;
        private readonly ILogger<AzureOpenAILanguageModel> _logger;
        private readonly HttpClient _httpClient;


        /// <summary>
        /// Azure OpenAI API version to use.
        /// </summary>
        /// <remarks>
        /// Azure OpenAI uses dated API versions. This should be updated periodically
        /// to use the latest stable version. Check the Azure OpenAI documentation
        /// for the current recommended version.
        /// </remarks>
        private const string ApiVersion = "2024-02-15-preview";


        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };


        /// <summary>
        /// Gets the capabilities of this language model.
        /// </summary>
        public IModelCapabilities Capabilities { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="AzureOpenAILanguageModel"/> class.
        /// </summary>
        /// <param name="apiKey">The Azure OpenAI API key.</param>
        /// <param name="model">The base model name (e.g., "gpt-4").</param>
        /// <param name="endpoint">The Azure OpenAI endpoint URL.</param>
        /// <param name="deploymentName">The deployment name in Azure.</param>
        /// <param name="timeout">The request timeout.</param>
        /// <param name="logger">Logger instance.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any required parameter is null.
        /// </exception>
        /// <remarks>
        /// The endpoint should be the base URL of your Azure OpenAI resource, e.g.:
        /// https://your-resource-name.openai.azure.com
        /// 
        /// The deployment name is the name you gave to your model deployment in the
        /// Azure portal. This can be different from the model name.
        /// </remarks>
        public AzureOpenAILanguageModel(
            string apiKey,
            string model,
            string endpoint,
            string deploymentName,
            TimeSpan timeout,
            ILogger<AzureOpenAILanguageModel> logger)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _endpoint = endpoint?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(endpoint));
            _deploymentName = deploymentName ?? model; // Use model name as deployment if not specified
            _timeout = timeout;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));


            _httpClient = new HttpClient
            {
                Timeout = timeout
            };


            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);


            Capabilities = new ModelCapabilities(
                maxContextTokens: GetMaxContextTokens(model),
                maxOutputTokens: GetMaxOutputTokens(model),
                supportsToolCalling: true,
                supportsVision: model.Contains("gpt-4") && model.Contains("vision"),
                supportsStreaming: true,
                supportedLanguages: new[] { "en", "es", "fr", "de", "it", "pt", "ja", "ko", "zh", "ru", "ar" });


            _logger.LogInformation(
                "Initialized Azure OpenAI model with endpoint {Endpoint}, deployment {Deployment}",
                _endpoint,
                _deploymentName);
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
        /// Azure OpenAI endpoint format:
        /// {endpoint}/openai/deployments/{deployment-name}/chat/completions?api-version={api-version}
        /// 
        /// The API is largely compatible with standard OpenAI, with the main
        /// difference being the endpoint structure and authentication method.
        /// 
        /// Azure-specific features:
        /// - Content filtering (can be configured per deployment)
        /// - Rate limiting based on deployment quota
        /// - Regional data residency
        /// </remarks>
        public async Task<IModelResponse> GenerateAsync(
            IModelRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));


            _logger.LogDebug(
                "Generating response with deployment {Deployment} for {MessageCount} messages",
                _deploymentName,
                request.Messages.Count());


            var requestUrl = BuildRequestUrl();
            var apiRequest = BuildApiRequest(request);
            var jsonRequest = JsonSerializer.Serialize(apiRequest, JsonOptions);


            _logger.LogTrace("API Request URL: {Url}", requestUrl);
            _logger.LogTrace("API Request Body: {Request}", jsonRequest);


            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
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
                        $"Azure OpenAI API request failed with status {httpResponse.StatusCode}: {responseContent}");
                }


                _logger.LogTrace("API Response: {Response}", responseContent);


                var apiResponse = JsonSerializer.Deserialize<AzureOpenAIApiResponse>(
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
        /// Azure OpenAI supports streaming with the same SSE format as standard OpenAI.
        /// This simplified implementation falls back to non-streaming.
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
        /// Builds the full request URL for the Azure OpenAI API.
        /// </summary>
        /// <returns>The complete request URL including deployment and API version.</returns>
        private string BuildRequestUrl()
        {
            return $"{_endpoint}/openai/deployments/{_deploymentName}/chat/completions?api-version={ApiVersion}";
        }


        /// <summary>
        /// Builds the API request object from the model request.
        /// </summary>
        /// <remarks>
        /// The request format is identical to standard OpenAI, so we can reuse
        /// the same request structure.
        /// </remarks>
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


        /// <summary>
        /// Builds the tool calls array for the API request.
        /// </summary>
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


        /// <summary>
        /// Builds the tools array for the API request.
        /// </summary>
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


        /// <summary>
        /// Converts the API response to a model response.
        /// </summary>
        private IModelResponse ConvertToModelResponse(AzureOpenAIApiResponse apiResponse)
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


        /// <summary>
        /// Extracts tool calls from the API response.
        /// </summary>
        private List<IToolCall> ExtractToolCalls(List<AzureOpenAIToolCall> toolCalls)
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


        /// <summary>
        /// Converts Azure OpenAI finish reason to our enum.
        /// </summary>
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


        /// <summary>
        /// Gets the maximum context tokens for a model.
        /// </summary>
        private int GetMaxContextTokens(string model)
        {
            return model.ToLowerInvariant() switch
            {
                var m when m.Contains("gpt-4-turbo") => 128000,
                var m when m.Contains("gpt-4-32k") => 32768,
                var m when m.Contains("gpt-4") => 8192,
                var m when m.Contains("gpt-35-turbo-16k") || m.Contains("gpt-3.5-turbo-16k") => 16384,
                var m when m.Contains("gpt-35") || m.Contains("gpt-3.5") => 4096,
                _ => 4096
            };
        }


        /// <summary>
        /// Gets the maximum output tokens for a model.
        /// </summary>
        private int GetMaxOutputTokens(string model)
        {
            return 4096; // Standard for most models
        }


        // API Response DTOs (same structure as OpenAI)
        private class AzureOpenAIApiResponse
        {
            public string Id { get; set; }
            public string Object { get; set; }
            public long Created { get; set; }
            public string Model { get; set; }
            public List<AzureOpenAIChoice> Choices { get; set; }
            public AzureOpenAIUsage Usage { get; set; }
            public string SystemFingerprint { get; set; }
        }


        private class AzureOpenAIChoice
        {
            public int Index { get; set; }
            public AzureOpenAIMessage Message { get; set; }
            public string FinishReason { get; set; }
        }


        private class AzureOpenAIMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
            public List<AzureOpenAIToolCall> ToolCalls { get; set; }
        }


        private class AzureOpenAIToolCall
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public AzureOpenAIFunction Function { get; set; }
        }


        private class AzureOpenAIFunction
        {
            public string Name { get; set; }
            public string Arguments { get; set; }
        }


        private class AzureOpenAIUsage
        {
            public int PromptTokens { get; set; }
            public int CompletionTokens { get; set; }
            public int TotalTokens { get; set; }
        }
    }
}


