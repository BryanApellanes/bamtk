
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Bam.AI.Orchestration.Agent;


namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// Thread-safe registry for managing available tools.
    /// </summary>
    /// <remarks>
    /// The tool registry maintains a catalog of all tools available to the agent.
    /// Tools can be:
    /// - Registered at application startup (static tools)
    /// - Dynamically registered/unregistered at runtime
    /// - Discovered from MCP servers
    /// - Loaded from plugin assemblies
    /// 
    /// The registry provides efficient lookup by ID and filtering by capabilities.
    /// It uses a concurrent dictionary internally for thread-safe operations.
    /// 
    /// Thread Safety: All public methods are thread-safe and can be called
    /// concurrently from multiple threads.
    /// </remarks>
    /// <example>
    /// <code>
    /// var registry = new ToolRegistry(logger);
    /// 
    /// // Register a tool
    /// var weatherTool = new ToolDescriptor(
    ///     id: "weather_lookup",
    ///     name: "Weather Lookup",
    ///     description: "Get current weather for a location",
    ///     inputSchema: weatherSchema,
    ///     outputSchema: weatherOutputSchema,
    ///     capabilities: ToolCapability.Read | ToolCapability.Search);
    /// 
    /// registry.RegisterTool(weatherTool);
    /// 
    /// // Find tools for an intent
    /// var tools = registry.FindToolsForIntent(dataRetrievalIntent);
    /// </code>
    /// </example>
    public class ToolRegistry : IToolRegistry
    {
        private readonly ConcurrentDictionary<string, IToolDescriptor> _tools;
        private readonly ILogger<ToolRegistry> _logger;


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolRegistry"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for diagnostics.</param>
        public ToolRegistry(ILogger<ToolRegistry> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tools = new ConcurrentDictionary<string, IToolDescriptor>(StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Gets all available tools in the registry.
        /// </summary>
        /// <returns>
        /// A collection of all registered tool descriptors. The collection is a
        /// snapshot and will not reflect subsequent changes to the registry.
        /// </returns>
        /// <remarks>
        /// Performance: O(n) where n is the number of registered tools.
        /// The returned collection is safe to enumerate even if tools are
        /// registered/unregistered concurrently.
        /// </remarks>
        public IEnumerable<IToolDescriptor> GetAvailableTools()
        {
            return _tools.Values.ToList();
        }


        /// <summary>
        /// Gets a specific tool by its ID.
        /// </summary>
        /// <param name="toolId">The unique tool identifier (case-insensitive).</param>
        /// <returns>The tool descriptor if found.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="toolId"/> is null or empty.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no tool with the specified ID exists.
        /// </exception>
        /// <remarks>
        /// Performance: O(1) average case for lookup.
        /// Tool IDs are case-insensitive, so "WeatherTool" and "weathertool" refer
        /// to the same tool.
        /// </remarks>
        public IToolDescriptor GetTool(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId))
                throw new ArgumentException("ToolId cannot be null or empty", nameof(toolId));


            if (!_tools.TryGetValue(toolId, out var tool))
            {
                throw new KeyNotFoundException($"Tool with ID '{toolId}' not found in registry");
            }


            return tool;
        }


        /// <summary>
        /// Registers a new tool in the registry.
        /// </summary>
        /// <param name="tool">The tool descriptor to register.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tool"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a tool with the same ID is already registered.
        /// </exception>
        /// <remarks>
        /// If you need to update an existing tool, first unregister it, then
        /// register the updated version. This prevents accidental overwrites.
        /// 
        /// Best Practice: Register all static tools during application startup
        /// in dependency injection configuration. Only register tools dynamically
        /// for truly dynamic scenarios like MCP server connections.
        /// </remarks>
        public void RegisterTool(IToolDescriptor tool)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));


            if (!_tools.TryAdd(tool.Id, tool))
            {
                throw new InvalidOperationException(
                    $"Tool with ID '{tool.Id}' is already registered. Unregister it first if you want to update it.");
            }


            _logger.LogInformation(
                "Registered tool: {ToolId} ({ToolName}) with capabilities: {Capabilities}",
                tool.Id,
                tool.Name,
                tool.Capabilities);
        }


        /// <summary>
        /// Unregisters a tool from the registry.
        /// </summary>
        /// <param name="toolId">The ID of the tool to unregister.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="toolId"/> is null or empty.
        /// </exception>
        /// <remarks>
        /// This operation is idempotent - unregistering a non-existent tool
        /// does not throw an exception, it simply logs a warning.
        /// 
        /// Use this when:
        /// - Disconnecting from an MCP server
        /// - Unloading plugin assemblies
        /// - Disabling features that provide tools
        /// </remarks>
        public void UnregisterTool(string toolId)
        {
            if (string.IsNullOrWhiteSpace(toolId))
                throw new ArgumentException("ToolId cannot be null or empty", nameof(toolId));


            if (_tools.TryRemove(toolId, out var removed))
            {
                _logger.LogInformation(
                    "Unregistered tool: {ToolId} ({ToolName})",
                    removed.Id,
                    removed.Name);
            }
            else
            {
                _logger.LogWarning(
                    "Attempted to unregister non-existent tool: {ToolId}",
                    toolId);
            }
        }


        /// <summary>
        /// Finds tools that match the given intent.
        /// </summary>
        /// <param name="intent">The prompt intent to match against.</param>
        /// <returns>
        /// A collection of tool descriptors that are relevant for the intent,
        /// ordered by relevance (most relevant first).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="intent"/> is null.
        /// </exception>
        /// <remarks>
        /// Tool selection uses multiple heuristics:
        /// 1. Intent type matching (DataRetrieval → Search tools)
        /// 2. Entity matching (URL entity → Web fetch tools)
        /// 3. Keyword matching in tool descriptions
        /// 4. Capability requirements
        /// 
        /// The relevance scoring ensures that the most appropriate tools are
        /// returned first, allowing the tool selector to prioritize them.
        /// 
        /// Performance: O(n) where n is the number of registered tools.
        /// For large tool registries (100+ tools), consider implementing
        /// an indexing mechanism for faster lookups.
        /// </remarks>
        public IEnumerable<IToolDescriptor> FindToolsForIntent(IPromptIntent intent)
        {
            if (intent == null)
                throw new ArgumentNullException(nameof(intent));


            var scoredTools = new List<(IToolDescriptor Tool, double Score)>();


            foreach (var tool in _tools.Values)
            {
                var score = CalculateRelevanceScore(tool, intent);
                if (score > 0)
                {
                    scoredTools.Add((tool, score));
                }
            }


            var results = scoredTools
                .OrderByDescending(t => t.Score)
                .Select(t => t.Tool)
                .ToList();


            _logger.LogDebug(
                "Found {Count} tools for intent type {IntentType}",
                results.Count,
                intent.Type);


            return results;
        }


        /// <summary>
        /// Calculates a relevance score for a tool given an intent.
        /// </summary>
        /// <param name="tool">The tool to score.</param>
        /// <param name="intent">The intent to match against.</param>
        /// <returns>A score between 0.0 (not relevant) and 1.0 (highly relevant).</returns>
        private double CalculateRelevanceScore(IToolDescriptor tool, IPromptIntent intent)
        {
            double score = 0.0;


            // Intent type matching
            switch (intent.Type)
            {
                case IntentType.DataRetrieval:
                    if (tool.Capabilities.HasFlag(ToolCapability.Search) ||
                        tool.Capabilities.HasFlag(ToolCapability.Read))
                    {
                        score += 0.5;
                    }
                    break;


                case IntentType.Command:
                    if (tool.Capabilities.HasFlag(ToolCapability.Write) ||
                        tool.Capabilities.HasFlag(ToolCapability.Execute))
                    {
                        score += 0.5;
                    }
                    break;


                case IntentType.Computation:
                    if (tool.Capabilities.HasFlag(ToolCapability.Transform) ||
                        tool.Description.Contains("calculate", StringComparison.OrdinalIgnoreCase))
                    {
                        score += 0.5;
                    }
                    break;


                case IntentType.Analysis:
                    if (tool.Capabilities.HasFlag(ToolCapability.Read) ||
                        tool.Capabilities.HasFlag(ToolCapability.Transform))
                    {
                        score += 0.4;
                    }
                    break;
            }


            // Entity-based matching
            foreach (var entity in intent.ExtractedEntities)
            {
                switch (entity.Type.ToLowerInvariant())
                {
                    case "url":
                        if (tool.Description.Contains("web", StringComparison.OrdinalIgnoreCase) ||
                            tool.Description.Contains("http", StringComparison.OrdinalIgnoreCase))
                        {
                            score += 0.3;
                        }
                        break;


                    case "filepath":
                        if (tool.Description.Contains("file", StringComparison.OrdinalIgnoreCase))
                        {
                            score += 0.3;
                        }
                        break;


                    case "email":
                        if (tool.Description.Contains("email", StringComparison.OrdinalIgnoreCase) ||
                            tool.Description.Contains("mail", StringComparison.OrdinalIgnoreCase))
                        {
                            score += 0.3;
                        }
                        break;


                    case "date":
                    case "time":
                        if (tool.Description.Contains("calendar", StringComparison.OrdinalIgnoreCase) ||
                            tool.Description.Contains("schedule", StringComparison.OrdinalIgnoreCase))
                        {
                            score += 0.2;
                        }
                        break;
                }
            }


            // Keyword matching in summary
            var summaryWords = intent.Summary.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var descriptionWords = tool.Description.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var matchingWords = summaryWords.Intersect(
                descriptionWords,
                StringComparer.OrdinalIgnoreCase).Count();


            if (matchingWords > 0)
            {
                score += Math.Min(0.3, matchingWords * 0.1);
            }


            return Math.Clamp(score, 0.0, 1.0);
        }
    }

}