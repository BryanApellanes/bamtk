using Bam.AI.Orchestration.Core;
using Bam.AI.Orchestration.Model;
using Bam.AI.Orchestration.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Bam.AI.Orchestration.Monitoring
{
    /// <summary>
    /// Tracks and logs orchestration metrics for monitoring and analytics.
    /// </summary>
    /// <remarks>
    /// The orchestration monitor provides observability into the AI system's
    /// operation by tracking:
    /// - Request rates and volumes
    /// - Response times and latencies
    /// - Tool execution patterns
    /// - Error rates and types
    /// - Resource utilization
    /// 
    /// This data is essential for:
    /// - Performance optimization
    /// - Cost management
    /// - Quality assurance
    /// - Debugging and troubleshooting
    /// - Capacity planning
    /// 
    /// In production, integrate with:
    /// - Application Insights
    /// - Prometheus/Grafana
    /// - DataDog
    /// - New Relic
    /// - CloudWatch
    /// 
    /// Thread Safety: This class is thread-safe. Multiple threads can record
    /// metrics concurrently.
    /// </remarks>
    /// <example>
    /// <code>
    /// var monitor = new OrchestrationMonitor(logger);
    /// monitor.TrackPrompt(prompt);
    /// 
    /// // ... process prompt ...
    /// 
    /// monitor.TrackResponse(response, duration);
    /// 
    /// var metrics = await monitor.GetMetricsAsync(startTime, endTime);
    /// Console.WriteLine($"Total prompts: {metrics.TotalPrompts}");
    /// Console.WriteLine($"Average response time: {metrics.AverageResponseTime}");
    /// </code>
    /// </example>
    public class OrchestrationMonitor : IOrchestrationMonitor
    // : IOrchestrationMonitor
    {
        private readonly ILogger<OrchestrationMonitor> _logger;

        // Thread-safe collections for tracking metrics
        private readonly ConcurrentBag<PromptMetric> _promptMetrics;
        private readonly ConcurrentBag<ResponseMetric> _responseMetrics;
        private readonly ConcurrentBag<ToolExecutionMetric> _toolMetrics;
        private readonly ConcurrentBag<ErrorMetric> _errorMetrics;


        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationMonitor"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for diagnostics.</param>
        public OrchestrationMonitor(ILogger<OrchestrationMonitor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _promptMetrics = new ConcurrentBag<PromptMetric>();
            _responseMetrics = new ConcurrentBag<ResponseMetric>();
            _toolMetrics = new ConcurrentBag<ToolExecutionMetric>();
            _errorMetrics = new ConcurrentBag<ErrorMetric>();
        }


        /// <summary>
        /// Tracks a prompt submission.
        /// </summary>
        /// <param name="prompt">The prompt being tracked.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="prompt"/> is null.
        /// </exception>
        /// <remarks>
        /// Records metrics about the prompt including:
        /// - Timestamp
        /// - User ID
        /// - Content length
        /// - Attachment count and sizes
        /// - Metadata
        /// 
        /// This data helps analyze usage patterns, popular features, and
        /// potential abuse or anomalies.
        /// </remarks>
        public void TrackPrompt(IPrompt prompt)
        {
            if (prompt == null)
                throw new ArgumentNullException(nameof(prompt));


            var metric = new PromptMetric
            {
                Timestamp = DateTime.UtcNow,
                UserId = prompt.UserId,
                ContentLength = prompt.Content.Length,
                AttachmentCount = prompt.Attachments.Count(),
                TotalAttachmentSize = prompt.Attachments.Sum(a => a.Size)
            };


            _promptMetrics.Add(metric);


            _logger.LogInformation(
                "Tracked prompt from user {UserId}: {Length} chars, {Attachments} attachments",
                prompt.UserId,
                metric.ContentLength,
                metric.AttachmentCount);
        }


        /// <summary>
        /// Tracks a response generation.
        /// </summary>
        /// <param name="response">The response being tracked.</param>
        /// <param name="duration">How long the response took to generate.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="response"/> is null.
        /// </exception>
        /// <remarks>
        /// Records metrics about the response including:
        /// - Generation time
        /// - Status (success/failure)
        /// - Content length
        /// - Tool calls made
        /// - Token usage (if available)
        /// 
        /// Response time is a critical metric for user experience and should
        /// be monitored closely. Set alerts for:
        /// - P50 > 2 seconds
        /// - P95 > 5 seconds
        /// - P99 > 10 seconds
        /// </remarks>
        public void TrackResponse(IAgentResponse response, TimeSpan duration)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));


            var metric = new ResponseMetric
            {
                Timestamp = DateTime.UtcNow,
                Duration = duration,
                Status = response.Status,
                ContentLength = response.Content.Length,
                ToolCallCount = response.ToolCallsExecuted.Count(),
                TokenUsage = ExtractTokenUsage(response.Metadata)
            };


            _responseMetrics.Add(metric);


            _logger.LogInformation(
                "Tracked response: {Status} in {Duration:F2}s, {ToolCalls} tool calls",
                response.Status,
                duration.TotalSeconds,
                metric.ToolCallCount);


            // Alert on slow responses
            if (duration.TotalSeconds > 10)
            {
                _logger.LogWarning(
                    "Slow response detected: {Duration:F2}s (threshold: 10s)",
                    duration.TotalSeconds);
            }
        }


        /// <summary>
        /// Tracks a tool execution.
        /// </summary>
        /// <param name="toolCall">The tool call being tracked.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="toolCall"/> is null.
        /// </exception>
        /// <remarks>
        /// Records metrics about tool usage including:
        /// - Which tools are used most frequently
        /// - Tool success/failure rates
        /// - Average execution times per tool
        /// - Common error patterns
        /// 
        /// This data helps:
        /// - Identify unreliable tools
        /// - Optimize tool selection
        /// - Plan infrastructure capacity
        /// - Debug integration issues
        /// </remarks>
        public void TrackToolExecution(IToolCall toolCall)
        {
            if (toolCall == null)
                throw new ArgumentNullException(nameof(toolCall));


            var metric = new ToolExecutionMetric
            {
                Timestamp = DateTime.UtcNow,
                ToolId = toolCall.ToolId,
                ToolName = toolCall.ToolName,
                Success = toolCall.Result?.Success ?? false,
                Duration = toolCall.Result?.ExecutionDuration ?? TimeSpan.Zero,
                ErrorMessage = toolCall.Result?.ErrorMessage
            };


            _toolMetrics.Add(metric);


            _logger.LogInformation(
                "Tracked tool execution: {ToolName} - {Status} in {Duration:F2}s",
                toolCall.ToolName,
                metric.Success ? "Success" : "Failed",
                metric.Duration.TotalSeconds);
        }


        /// <summary>
        /// Tracks an error occurrence.
        /// </summary>
        /// <param name="exception">The exception that occurred.</param>
        /// <param name="context">Context information about where the error occurred.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="exception"/> is null.
        /// </exception>
        /// <remarks>
        /// Records detailed error information for debugging and alerting:
        /// - Exception type and message
        /// - Stack trace
        /// - Contextual information
        /// - Timestamp and frequency
        /// 
        /// High error rates should trigger alerts. Recommended thresholds:
        /// - Error rate > 1% of total requests
        /// - Same error > 10 times in 5 minutes
        /// - Any critical errors
        /// </remarks>
        public void TrackError(Exception exception, string context)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));


            var metric = new ErrorMetric
            {
                Timestamp = DateTime.UtcNow,
                ExceptionType = exception.GetType().Name,
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Context = context ?? "Unknown"
            };


            _errorMetrics.Add(metric);


            _logger.LogError(
                exception,
                "Tracked error in context: {Context}",
                context);


            // Alert on critical errors
            if (exception is OutOfMemoryException ||
                exception is StackOverflowException ||
                exception.Message.Contains("critical", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogCritical(
                    exception,
                    "CRITICAL ERROR detected in context: {Context}",
                    context);
            }
        }


        /// <summary>
        /// Gets aggregated metrics for a time period.
        /// </summary>
        /// <param name="startTime">The start of the time period.</param>
        /// <param name="endTime">The end of the time period.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// aggregated metrics for the specified time period.
        /// </returns>
        /// <remarks>
        /// This method computes various statistics over the time period:
        /// - Total request counts
        /// - Success/failure rates
        /// - Percentile response times (P50, P95, P99)
        /// - Tool usage distribution
        /// - Error distribution
        /// 
        /// Performance: This operation scans all stored metrics. For production
        /// systems with high traffic, consider:
        /// - Pre-aggregating metrics periodically
        /// - Using a time-series database
        /// - Implementing metric retention policies
        /// </remarks>
        public Task<IOrchestrationMetrics> GetMetricsAsync(DateTime startTime, DateTime endTime)
        {
            _logger.LogDebug(
                "Computing metrics for period {Start} to {End}",
                startTime,
                endTime);


            // Filter metrics by time period
            var promptsInPeriod = _promptMetrics
                .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                .ToList();


            var responsesInPeriod = _responseMetrics
                .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                .ToList();


            var toolsInPeriod = _toolMetrics
                .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                .ToList();


            var errorsInPeriod = _errorMetrics
                .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                .ToList();


            // Compute aggregated metrics
            var totalPrompts = promptsInPeriod.Count;
            var successfulResponses = responsesInPeriod.Count(r => r.Status == ResponseStatus.Success);
            var failedResponses = responsesInPeriod.Count(r => r.Status == ResponseStatus.Failed);


            var averageResponseTime = responsesInPeriod.Any()
                ? TimeSpan.FromMilliseconds(responsesInPeriod.Average(r => r.Duration.TotalMilliseconds))
                : TimeSpan.Zero;


            var toolUsageCount = toolsInPeriod
                .GroupBy(t => t.ToolName)
                .ToDictionary(g => g.Key, g => g.Count());


            var intentDistribution = responsesInPeriod
                .Select(r => r.Status.ToString())
                .GroupBy(s => s)
                .ToDictionary(g => g.Key, g => g.Count());


            var metrics = new OrchestrationMetrics(
                totalPrompts: totalPrompts,
                successfulResponses: successfulResponses,
                failedResponses: failedResponses,
                averageResponseTime: averageResponseTime,
                toolUsageCount: toolUsageCount,
                intentDistribution: intentDistribution);


            _logger.LogInformation(
                "Computed metrics: {Total} prompts, {Success} successful, {Failed} failed, avg time {AvgTime:F2}s",
                totalPrompts,
                successfulResponses,
                failedResponses,
                averageResponseTime.TotalSeconds);


            return Task.FromResult<IOrchestrationMetrics>(metrics);
        }


        /// <summary>
        /// Extracts token usage from response metadata.
        /// </summary>
        private TokenUsageData ExtractTokenUsage(IDictionary<string, object> metadata)
        {
            if (metadata == null || !metadata.ContainsKey("token_usage"))
                return null;


            try
            {
                var usage = metadata["token_usage"] as IUsageMetrics;
                if (usage != null)
                {
                    return new TokenUsageData
                    {
                        PromptTokens = usage.PromptTokens,
                        CompletionTokens = usage.CompletionTokens,
                        TotalTokens = usage.TotalTokens
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract token usage from metadata");
            }


            return null;
        }


        // Internal metric classes
        private class PromptMetric
        {
            public DateTime Timestamp { get; set; }
            public string UserId { get; set; }
            public int ContentLength { get; set; }
            public int AttachmentCount { get; set; }
            public long TotalAttachmentSize { get; set; }
        }


        private class ResponseMetric
        {
            public DateTime Timestamp { get; set; }
            public TimeSpan Duration { get; set; }
            public ResponseStatus Status { get; set; }
            public int ContentLength { get; set; }
            public int ToolCallCount { get; set; }
            public TokenUsageData TokenUsage { get; set; }
        }


        private class ToolExecutionMetric
        {
            public DateTime Timestamp { get; set; }
            public string ToolId { get; set; }
            public string ToolName { get; set; }
            public bool Success { get; set; }
            public TimeSpan Duration { get; set; }
            public string ErrorMessage { get; set; }
        }


        private class ErrorMetric
        {
            public DateTime Timestamp { get; set; }
            public string ExceptionType { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
            public string Context { get; set; }
        }


        private class TokenUsageData
        {
            public int PromptTokens { get; set; }
            public int CompletionTokens { get; set; }
            public int TotalTokens { get; set; }
        }
    }

}
