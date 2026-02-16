using Bam.AI.Orchestration.DependencyInjection;
using Bam.AI.Orchestration.Examples;
using Bam.AI.Orchestration.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace AI.Orchestration.Examples
{
    /// <summary>
    /// Example application demonstrating the AI orchestration system.
    /// </summary>
    /// <remarks>
    /// This example shows:
    /// 1. How to configure the application with appsettings.json
    /// 2. How to register services with dependency injection
    /// 3. How to create and register custom tools
    /// 4. How to process prompts and handle responses
    /// 5. How to maintain conversation context
    /// 6. How to monitor metrics
    /// 
    /// Run this application with:
    /// dotnet run --project AI.Orchestration.Examples
    /// </remarks>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== AI Orchestration System Example ===");
            Console.WriteLine();


            // Build the host
            var host = CreateHostBuilder(args).Build();


            // Run example scenarios
            await RunExamplesAsync(host.Services);


            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }


        /// <summary>
        /// Creates and configures the host builder.
        /// </summary>
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Load configuration from appsettings.json
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                        optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    // Configure logging
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.AddDebug();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });


                    // Register AI orchestration services
                    services.AddAIOrchestration(context.Configuration);


                    // Register custom tools
                    RegisterCustomTools(services);


                    // Register example service
                    services.AddSingleton<ExampleOrchestrationService>();
                });


        /// <summary>
        /// Registers custom tool implementations and descriptors.
        /// </summary>
        static void RegisterCustomTools(IServiceCollection services)
        {
            // Register Weather API Tool
            var weatherSchema = new ToolSchema(
                schemaType: "json-schema",
                schemaDefinition: @"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""location"": {
                            ""type"": ""string"",
                            ""description"": ""The city and state, e.g. San Francisco, CA""
                        },
                        ""unit"": {
                            ""type"": ""string"",
                            ""enum"": [""celsius"", ""fahrenheit""],
                            ""description"": ""The temperature unit""
                        }
                    },
                    ""required"": [""location""]
                }",
                parameters: new[]
                {
                    new Parameter(
                        name: "location",
                        type: "string",
                        required: true,
                        description: "The city and state"),
                    new Parameter(
                        name: "unit",
                        type: "string",
                        required: false,
                        defaultValue: "fahrenheit",
                        description: "Temperature unit",
                        allowedValues: new[] { "celsius", "fahrenheit" })
                });


            var weatherOutputSchema = new ToolSchema(
                schemaType: "json-schema",
                schemaDefinition: @"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""temperature"": { ""type"": ""number"" },
                        ""conditions"": { ""type"": ""string"" },
                        ""humidity"": { ""type"": ""number"" }
                    }
                }",
                parameters: Array.Empty<IParameter>());


            var weatherDescriptor = new ToolDescriptor(
                id: "weather_api",
                name: "Weather API",
                description: "Gets current weather information for a specified location",
                inputSchema: weatherSchema,
                outputSchema: weatherOutputSchema,
                capabilities: ToolCapability.Read | ToolCapability.Search,
                requiresAuthorization: false);


            services.AddTool<WeatherApiTool>("weather_api");
            services.AddToolDescriptor(weatherDescriptor);


            // Register Calculator Tool
            var calculatorSchema = new ToolSchema(
                schemaType: "json-schema",
                schemaDefinition: @"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""expression"": {
                            ""type"": ""string"",
                            ""description"": ""Mathematical expression to evaluate""
                        }
                    },
                    ""required"": [""expression""]
                }",
                parameters: new[]
                {
                    new Parameter(
                        name: "expression",
                        type: "string",
                        required: true,
                        description: "Mathematical expression (e.g., '2 + 2', '10 * 5 + 3')")
                });


            var calculatorOutputSchema = new ToolSchema(
                schemaType: "json-schema",
                schemaDefinition: @"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""result"": { ""type"": ""number"" }
                    }
                }",
                parameters: Array.Empty<IParameter>());


            var calculatorDescriptor = new ToolDescriptor(
                id: "calculator",
                name: "Calculator",
                description: "Evaluates mathematical expressions",
                inputSchema: calculatorSchema,
                outputSchema: calculatorOutputSchema,
                capabilities: ToolCapability.Transform,
                requiresAuthorization: false);


            services.AddTool<CalculatorTool>("calculator");
            services.AddToolDescriptor(calculatorDescriptor);


            // Register Database Query Tool
            var databaseSchema = new ToolSchema(
                schemaType: "json-schema",
                schemaDefinition: @"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""query"": {
                            ""type"": ""string"",
                            ""description"": ""SQL query to execute""
                        },
                        ""database"": {
                            ""type"": ""string"",
                            ""description"": ""Database name""
                        }
                    },
                    ""required"": [""query"", ""database""]
                }",
                parameters: new[]
                {
                    new Parameter(
                        name: "query",
                        type: "string",
                        required: true,
                        description: "SQL SELECT query"),
                    new Parameter(
                        name: "database",
                        type: "string",
                        required: true,
                        description: "Database name")
                });


            var databaseOutputSchema = new ToolSchema(
                schemaType: "json-schema",
                schemaDefinition: @"{
                    ""type"": ""object"",
                    ""properties"": {
                        ""rows"": { ""type"": ""array"" },
                        ""count"": { ""type"": ""number"" }
                    }
                }",
                parameters: Array.Empty<IParameter>());


            var databaseDescriptor = new ToolDescriptor(
                id: "database_query",
                name: "Database Query",
                description: "Executes read-only SQL queries against databases",
                inputSchema: databaseSchema,
                outputSchema: databaseOutputSchema,
                capabilities: ToolCapability.Read | ToolCapability.Search,
                requiresAuthorization: true);


            services.AddTool<DatabaseQueryTool>("database_query");
            services.AddToolDescriptor(databaseDescriptor);
        }


        /// <summary>
        /// Runs example scenarios demonstrating the orchestration system.
        /// </summary>
        static async Task RunExamplesAsync(IServiceProvider services)
        {
            var exampleService = services.GetRequiredService<ExampleOrchestrationService>();


            Console.WriteLine("Running example scenarios...");
            Console.WriteLine();


            // Example 1: Simple question without tools
            await exampleService.RunSimpleQuestionExample();
            Console.WriteLine();


            // Example 2: Weather query with tool usage
            await exampleService.RunWeatherQueryExample();
            Console.WriteLine();


            // Example 3: Multi-turn conversation
            await exampleService.RunMultiTurnConversationExample();
            Console.WriteLine();


            // Example 4: Complex calculation
            await exampleService.RunCalculationExample();
            Console.WriteLine();


            // Example 5: Error handling
            await exampleService.RunErrorHandlingExample();
            Console.WriteLine();


            // Example 6: Metrics reporting
            await exampleService.RunMetricsExample();
        }
    }


}