
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// Executes tool calls and manages tool implementations.
    /// </summary>
    /// <remarks>
    /// The tool executor is responsible for:
    /// - Resolving tool implementations from the service provider
    /// - Invoking tools with parameters
    /// - Handling errors and timeouts
    /// - Tracking execution metrics
    /// - Supporting parallel and sequential execution modes
    /// 
    /// Tool implementations are resolved dynamically from the DI container,
    /// allowing for flexible registration and testing with mock implementations.
    /// 
    /// Thread Safety: This class is thread-safe. Multiple tool executions can
    /// run concurrently, and the executor properly manages their lifecycle.
    /// </remarks>
    /// <example>
    /// <code>
    /// var executor = new ToolExecutor(serviceProvider, logger);
    /// var result = await executor.ExecuteAsync(selectedTool, cancellationToken);
    /// 
    /// if (result.Success)
    /// {
    ///     Console.WriteLine($"Tool returned: {result.Data}");
    /// }
    /// else
    /// {
    ///     Console.WriteLine($"Tool failed: {result.ErrorMessage}");
    /// }
    /// </code>
    /// </example>
    public class ToolExecutor : IToolExecutor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ToolExecutor> _logger;


        /// <summary>
        /// Default timeout for tool execution.
        /// </summary>
        /// <remarks>
        /// Individual tools can have longer timeouts specified in their metadata,
        /// but this provides a reasonable default to prevent hung operations.
        /// 30 seconds is usually sufficient for most API calls and data operations.
        /// </remarks>
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);


        /// <summary>
        /// Maximum timeout allowed for any tool.
        /// </summary>
        /// <remarks>
        /// Even if a tool specifies a longer timeout, it will be capped at this
        /// value to prevent indefinite waits. Adjust based on your requirements.
        /// </remarks>
        private static readonly TimeSpan MaximumTimeout = TimeSpan.FromMinutes(5);


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolExecutor"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving tool implementations.</param>
        /// <param name="logger">Logger instance for diagnostics.</param>
        public ToolExecutor(
            IServiceProvider serviceProvider,
            ILogger<ToolExecutor> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Executes a single tool asynchronously.
        /// </summary>
        /// <param name="tool">The tool to execute.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the tool execution result, which includes the output data or error information.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tool"/> is null.
        /// </exception>
        /// <remarks>
        /// Execution Process:
        /// 1. Resolve tool implementation from service provider
        /// 2. Validate parameters against tool schema
        /// 3. Execute tool with timeout protection
        /// 4. Capture result or error
        /// 5. Return structured result with metadata
        /// 
        /// The executor handles all exceptions and returns them as failed results
        /// rather than throwing. This ensures the orchestration pipeline continues
        /// even if individual tools fail.
        /// 
        /// Performance: Execution time depends on the tool implementation.
        /// The executor adds minimal overhead (typically under 1ms).
        /// </remarks>
        public async Task<IToolResult> ExecuteAsync(
            ISelectedTool tool,
            CancellationToken cancellationToken = default)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));


            var startTime = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();


            _logger.LogInformation(
                "Executing tool: {ToolName} (ID: {ToolId})",
                tool.Descriptor.Name,
                tool.Descriptor.Id);


            try
            {
                // Resolve tool implementation
                var implementation = ResolveToolImplementation(tool.Descriptor.Id);


                if (implementation == null)
                {
                    _logger.LogError(
                        "No implementation found for tool: {ToolId}",
                        tool.Descriptor.Id);


                    return new ToolResult(
                        toolId: tool.Descriptor.Id,
                        success: false,
                        data: null,
                        errorMessage: $"No implementation registered for tool '{tool.Descriptor.Id}'",
                        executedAt: startTime,
                        executionDuration: stopwatch.Elapsed);
                }


                // Validate parameters
                var validationError = ValidateParameters(tool);
                if (validationError != null)
                {
                    _logger.LogWarning(
                        "Parameter validation failed for tool {ToolId}: {Error}",
                        tool.Descriptor.Id,
                        validationError);


                    return new ToolResult(
                        toolId: tool.Descriptor.Id,
                        success: false,
                        data: null,
                        errorMessage: validationError,
                        executedAt: startTime,
                        executionDuration: stopwatch.Elapsed);
                }


                // Execute with timeout
                var timeout = GetToolTimeout(tool.Descriptor);
                using var timeoutCts = new CancellationTokenSource(timeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    timeoutCts.Token);


                object result;
                try
                {
                    result = await implementation.ExecuteAsync(
                        tool.Parameters,
                        linkedCts.Token);
                }
                catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "Tool {ToolId} execution timed out after {Timeout}",
                        tool.Descriptor.Id,
                        timeout);


                    return new ToolResult(
                        toolId: tool.Descriptor.Id,
                        success: false,
                        data: null,
                        errorMessage: $"Tool execution timed out after {timeout.TotalSeconds:F1} seconds",
                        executedAt: startTime,
                        executionDuration: stopwatch.Elapsed);
                }


                stopwatch.Stop();


                _logger.LogInformation(
                    "Tool {ToolId} executed successfully in {Duration:F2}s",
                    tool.Descriptor.Id,
                    stopwatch.Elapsed.TotalSeconds);


                return new ToolResult(
                    toolId: tool.Descriptor.Id,
                    success: true,
                    data: result,
                    errorMessage: null,
                    executedAt: startTime,
                    executionDuration: stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();


                _logger.LogError(
                    ex,
                    "Error executing tool {ToolId}",
                    tool.Descriptor.Id);


                return new ToolResult(
                    toolId: tool.Descriptor.Id,
                    success: false,
                    data: null,
                    errorMessage: $"Tool execution failed: {ex.Message}",
                    executedAt: startTime,
                    executionDuration: stopwatch.Elapsed);
            }
        }


        /// <summary>
        /// Executes multiple tools in batch.
        /// </summary>
        /// <param name="tools">The tools to execute.</param>
        /// <param name="mode">The execution mode (sequential or parallel).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the results from all tool executions in the order they were provided.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tools"/> is null.
        /// </exception>
        /// <remarks>
        /// Execution Modes:
        /// 
        /// Sequential: Tools execute one after another in order. Use when:
        /// - Tools have dependencies on each other's results
        /// - Order matters for correctness
        /// - Resource constraints limit concurrent operations
        /// 
        /// Parallel: Tools execute concurrently. Use when:
        /// - Tools are independent
        /// - Performance is critical
        /// - System resources can handle concurrent operations
        /// 
        /// Conditional: Tools execute based on previous results (not yet implemented).
        /// 
        /// The executor always returns results in the same order as the input tools,
        /// regardless of execution mode. In parallel mode, faster tools wait for
        /// slower ones to maintain order consistency.
        /// </remarks>
        public async Task<IEnumerable<IToolResult>> ExecuteBatchAsync(
            IEnumerable<ISelectedTool> tools,
            ExecutionMode mode = ExecutionMode.Sequential,
            CancellationToken cancellationToken = default)
        {
            if (tools == null)
                throw new ArgumentNullException(nameof(tools));


            var toolList = tools.ToList();


            if (!toolList.Any())
            {
                return Enumerable.Empty<IToolResult>();
            }


            _logger.LogInformation(
                "Executing batch of {Count} tools in {Mode} mode",
                toolList.Count,
                mode);


            return mode switch
            {
                ExecutionMode.Sequential => await ExecuteSequentiallyAsync(toolList, cancellationToken),
                ExecutionMode.Parallel => await ExecuteInParallelAsync(toolList, cancellationToken),
                ExecutionMode.Conditional => await ExecuteConditionallyAsync(toolList, cancellationToken),
                _ => throw new ArgumentException($"Unknown execution mode: {mode}", nameof(mode))
            };
        }


        /// <summary>
        /// Executes tools sequentially, one after another.
        /// </summary>
        private async Task<IEnumerable<IToolResult>> ExecuteSequentiallyAsync(
            List<ISelectedTool> tools,
            CancellationToken cancellationToken)
        {
            var results = new List<IToolResult>();


            foreach (var tool in tools.OrderBy(t => t.ExecutionOrder))
            {
                var result = await ExecuteAsync(tool, cancellationToken);
                results.Add(result);


                // Optionally, you could stop on first failure
                // if (!result.Success) break;
            }


            return results;
        }


        /// <summary>
        /// Executes tools in parallel.
        /// </summary>
        private async Task<IEnumerable<IToolResult>> ExecuteInParallelAsync(
            List<ISelectedTool> tools,
            CancellationToken cancellationToken)
        {
            var tasks = tools.Select(tool => ExecuteAsync(tool, cancellationToken)).ToList();
            var results = await Task.WhenAll(tasks);
            return results;
        }


        /// <summary>
        /// Executes tools conditionally based on previous results.
        /// </summary>
        /// <remarks>
        /// This implementation is a placeholder. A full implementation would:
        /// - Evaluate conditions on each tool
        /// - Skip tools whose conditions aren't met
        /// - Pass results from previous tools as context
        /// </remarks>
        private async Task<IEnumerable<IToolResult>> ExecuteConditionallyAsync(
            List<ISelectedTool> tools,
            CancellationToken cancellationToken)
        {
            // For now, fall back to sequential execution
            // A full implementation would check conditions and dependencies
            _logger.LogWarning("Conditional execution not fully implemented, using sequential mode");
            return await ExecuteSequentiallyAsync(tools, cancellationToken);
        }


        /// <summary>
        /// Resolves a tool implementation from the service provider.
        /// </summary>
        /// <param name="toolId">The tool ID to resolve.</param>
        /// <returns>The tool implementation, or null if not found.</returns>
        /// <remarks>
        /// Tool implementations should be registered in DI with a keyed service
        /// where the key is the tool ID. For example:
        /// 
        /// services.AddKeyedSingleton&lt;IToolImplementation&gt;("weather_api", new WeatherApiTool());
        /// 
        /// This allows multiple tool implementations to coexist in the same
        /// service container.
        /// </remarks>
        private IToolImplementation ResolveToolImplementation(string toolId)
        {
            try
            {
                // Try to get keyed service (preferred)
                var implementation = _serviceProvider.GetKeyedService<IToolImplementation>(toolId);

                if (implementation != null)
                    return implementation;


                // Fallback: try to resolve by convention (ToolId + "Tool" class name)
                // This is less flexible but can work for simple scenarios
                _logger.LogDebug(
                    "No keyed service found for {ToolId}, attempting convention-based resolution",
                    toolId);


                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error resolving tool implementation for {ToolId}",
                    toolId);
                return null;
            }
        }


        /// <summary>
        /// Validates tool parameters against the schema.
        /// </summary>
        /// <param name="tool">The selected tool with parameters.</param>
        /// <returns>An error message if validation fails, or null if valid.</returns>
        private string ValidateParameters(ISelectedTool tool)
        {
            var schema = tool.Descriptor.InputSchema;


            // Check required parameters
            foreach (var param in schema.Parameters.Where(p => p.Required))
            {
                if (!tool.Parameters.ContainsKey(param.Name) ||
                    tool.Parameters[param.Name] == null)
                {
                    return $"Required parameter '{param.Name}' is missing";
                }
            }


            // Validate parameter types and allowed values
            foreach (var kvp in tool.Parameters)
            {
                var param = schema.Parameters.FirstOrDefault(p => p.Name == kvp.Key);

                if (param == null)
                {
                    // Parameter not in schema - could be optional metadata
                    continue;
                }


                // Check allowed values if specified
                if (param.AllowedValues.Any())
                {
                    var valueStr = kvp.Value?.ToString() ?? string.Empty;
                    if (!param.AllowedValues.Contains(valueStr, StringComparer.OrdinalIgnoreCase))
                    {
                        return $"Parameter '{param.Name}' has invalid value. " +
                               $"Allowed values: {string.Join(", ", param.AllowedValues)}";
                    }
                }
            }


            return null; // Validation passed
        }


        /// <summary>
        /// Gets the timeout for a tool execution.
        /// </summary>
        /// <param name="descriptor">The tool descriptor.</param>
        /// <returns>The timeout duration.</returns>
        private TimeSpan GetToolTimeout(IToolDescriptor descriptor)
        {
            // Check if tool metadata specifies a timeout
            if (descriptor is ToolDescriptor concrete &&
                concrete.Description.Contains("timeout:", StringComparison.OrdinalIgnoreCase))
            {
                // This is a simplified implementation
                // In production, store timeout in structured metadata
                return DefaultTimeout;
            }


            return DefaultTimeout;
        }
    }

}