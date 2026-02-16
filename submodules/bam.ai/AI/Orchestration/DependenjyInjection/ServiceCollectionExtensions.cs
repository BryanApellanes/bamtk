using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Bam.AI.Orchestration.Agent;
using Bam.AI.Orchestration.Tools;
using Bam.AI.Orchestration.Memory;
using Bam.AI.Orchestration.Model;
using Bam.AI.Orchestration.Monitoring;
using Bam.AI.Orchestration.Planning;
using Bam.AI.Orchestration.Safety;


namespace Bam.AI.Orchestration.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring AI orchestration services.
    /// </summary>
    /// <remarks>
    /// This class provides convenient methods to register all AI orchestration
    /// components in the dependency injection container. It supports:
    /// - Automatic provider selection based on configuration
    /// - Flexible service lifetime management
    /// - Tool registration
    /// - Custom implementations
    /// 
    /// Usage in Startup.cs or Program.cs:
    /// services.AddAIOrchestration(configuration);
    /// </remarks>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all AI orchestration services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <remarks>
        /// This method registers:
        /// - Language model (based on configuration)
        /// - Orchestrator and all components
        /// - Tool registry and implementations
        /// - Safety validator
        /// - Monitor and memory manager
        /// 
        /// Example configuration in appsettings.json:
        /// {
        ///   "LanguageModel": {
        ///     "Provider": "Anthropic",
        ///     "ApiKey": "your-key",
        ///     "Model": "claude-sonnet-4-20250514"
        ///   }
        /// }
        /// </remarks>
        public static IServiceCollection AddAIOrchestration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));


            // Register language model factory and model
            services.AddSingleton<LanguageModelFactory>();
            services.AddSingleton<ILanguageModel>(sp =>
            {
                var factory = sp.GetRequiredService<LanguageModelFactory>();
                return factory.CreateLanguageModel();
            });


            // Register core orchestration components
            services.AddSingleton<IPromptAnalyzer, PromptAnalyzer>();
            services.AddSingleton<IToolRegistry, ToolRegistry>();
            services.AddSingleton<IToolSelector, ToolSelector>();
            services.AddSingleton<IToolExecutor, ToolExecutor>();
            services.AddSingleton<ITaskPlanner, TaskPlanner>();

            // Register safety and monitoring
            services.AddSingleton<ISafetyValidator, SafetyValidator>();
            services.AddSingleton<IOrchestrationMonitor, OrchestrationMonitor>();
            services.AddSingleton<IMemoryManager, MemoryManager>();


            // Register the main orchestrator
            services.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();


            return services;
        }


        /// <summary>
        /// Adds AI orchestration services with a custom language model.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="modelFactory">Factory function to create the language model.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <remarks>
        /// Use this overload when you want to provide a custom language model
        /// implementation or configuration that doesn't fit the standard pattern.
        /// 
        /// Example:
        /// services.AddAIOrchestration(sp => 
        ///     new CustomLanguageModel(sp.GetRequiredService&lt;ILogger&lt;CustomLanguageModel&gt;&gt;()));
        /// </remarks>
        public static IServiceCollection AddAIOrchestration(
            this IServiceCollection services,
            Func<IServiceProvider, ILanguageModel> modelFactory)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (modelFactory == null)
                throw new ArgumentNullException(nameof(modelFactory));


            // Register custom language model
            services.AddSingleton(modelFactory);


            // Register core components
            services.AddSingleton<IPromptAnalyzer, PromptAnalyzer>();
            services.AddSingleton<IToolRegistry, ToolRegistry>();
            services.AddSingleton<IToolSelector, ToolSelector>();
            services.AddSingleton<IToolExecutor, ToolExecutor>();
            services.AddSingleton<ITaskPlanner, TaskPlanner>();
            services.AddSingleton<ISafetyValidator, SafetyValidator>();
            services.AddSingleton<IOrchestrationMonitor, OrchestrationMonitor>();
            services.AddSingleton<IMemoryManager, MemoryManager>();
            services.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();


            return services;
        }


        /// <summary>
        /// Registers a tool implementation in the service collection.
        /// </summary>
        /// <typeparam name="TImplementation">The tool implementation type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="toolId">The unique tool identifier.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <remarks>
        /// Tool implementations should implement IToolImplementation and be
        /// registered with their unique tool ID as the key.
        /// 
        /// Example:
        /// services.AddTool&lt;WeatherApiTool&gt;("weather_api");
        /// services.AddTool&lt;DatabaseQueryTool&gt;("database_query");
        /// </remarks>
        public static IServiceCollection AddTool<TImplementation>(
            this IServiceCollection services,
            string toolId)
            where TImplementation : class, IToolImplementation
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrWhiteSpace(toolId))
                throw new ArgumentException("Tool ID cannot be null or empty", nameof(toolId));


            services.AddKeyedSingleton<IToolImplementation, TImplementation>(toolId);
            return services;
        }


        /// <summary>
        /// Registers a tool descriptor in the tool registry.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="descriptor">The tool descriptor.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <remarks>
        /// Call this method after AddAIOrchestration to register tools.
        /// The tool descriptor defines the tool's metadata, while the
        /// implementation (registered with AddTool) provides the actual logic.
        /// 
        /// Example:
        /// var weatherDescriptor = new ToolDescriptor(
        ///     id: "weather_api",
        ///     name: "Weather API",
        ///     description: "Gets current weather for a location",
        ///     inputSchema: weatherSchema,
        ///     outputSchema: weatherOutputSchema,
        ///     capabilities: ToolCapability.Read | ToolCapability.Search);
        /// 
        /// services.AddToolDescriptor(weatherDescriptor);
        /// </remarks>
        public static IServiceCollection AddToolDescriptor(
            this IServiceCollection services,
            IToolDescriptor descriptor)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));


            // Register a startup action to add the tool to the registry
            services.AddHostedService<ToolRegistrationService>(sp =>
                new ToolRegistrationService(
                    sp.GetRequiredService<IToolRegistry>(),
                    descriptor));


            return services;
        }
    }


}