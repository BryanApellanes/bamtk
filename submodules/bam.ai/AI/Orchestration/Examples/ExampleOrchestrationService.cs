using Bam.AI.Orchestration.Agent;
using Bam.AI.Orchestration.Core.Models;
using Bam.AI.Orchestration.Monitoring;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service that demonstrates various orchestration scenarios.
/// </summary>
public class ExampleOrchestrationService
{
    private readonly IAgentOrchestrator _orchestrator;
    private readonly IOrchestrationMonitor _monitor;
    private readonly ILogger<ExampleOrchestrationService> _logger;


    public ExampleOrchestrationService(
        IAgentOrchestrator orchestrator,
        IOrchestrationMonitor monitor,
        ILogger<ExampleOrchestrationService> logger)
    {
        _orchestrator = orchestrator;
        _monitor = monitor;
        _logger = logger;
    }


    /// <summary>
    /// Example 1: Simple question that doesn't require tools.
    /// </summary>
    public async Task RunSimpleQuestionExample()
    {
        Console.WriteLine("--- Example 1: Simple Question ---");

        var prompt = new Prompt(
            content: "What is the capital of France?",
            userId: "example_user_1");


        Console.WriteLine($"User: {prompt.Content}");


        var response = await _orchestrator.ProcessPromptAsync(prompt);


        Console.WriteLine($"Assistant: {response.Content}");
        Console.WriteLine($"Status: {response.Status}");
        Console.WriteLine($"Tools Used: {response.ToolCallsExecuted.Count()}");
    }


    /// <summary>
    /// Example 2: Query requiring a tool (Weather API).
    /// </summary>
    public async Task RunWeatherQueryExample()
    {
        Console.WriteLine("--- Example 2: Weather Query (Tool Usage) ---");

        var prompt = new Prompt(
            content: "What's the weather like in Seattle right now?",
            userId: "example_user_2");


        Console.WriteLine($"User: {prompt.Content}");


        var response = await _orchestrator.ProcessPromptAsync(prompt);


        Console.WriteLine($"Assistant: {response.Content}");
        Console.WriteLine($"Status: {response.Status}");
        Console.WriteLine($"Tools Used: {response.ToolCallsExecuted.Count()}");

        foreach (var toolCall in response.ToolCallsExecuted)
        {
            Console.WriteLine($"  - {toolCall.ToolName}: {(toolCall.Result.Success ? "Success" : "Failed")}");
        }
    }


    /// <summary>
    /// Example 3: Multi-turn conversation with context.
    /// </summary>
    public async Task RunMultiTurnConversationExample()
    {
        Console.WriteLine("--- Example 3: Multi-Turn Conversation ---");

        var context = new ConversationContext("conversation_3");


        // Turn 1
        var prompt1 = new Prompt(
            content: "My name is Alice and I live in New York.",
            userId: "example_user_3");


        Console.WriteLine($"User: {prompt1.Content}");


        var response1 = await _orchestrator.ProcessPromptWithContextAsync(prompt1, context);


        Console.WriteLine($"Assistant: {response1.Content}");
        Console.WriteLine();


        // Turn 2 - Agent should remember the context
        var prompt2 = new Prompt(
            content: "What's the weather like where I live?",
            userId: "example_user_3");


        Console.WriteLine($"User: {prompt2.Content}");


        var response2 = await _orchestrator.ProcessPromptWithContextAsync(prompt2, context);


        Console.WriteLine($"Assistant: {response2.Content}");
        Console.WriteLine($"Context: {context.PreviousPrompts.Count()} previous exchanges");
    }


    /// <summary>
    /// Example 4: Complex calculation using the calculator tool.
    /// </summary>
    public async Task RunCalculationExample()
    {
        Console.WriteLine("--- Example 4: Complex Calculation ---");

        var prompt = new Prompt(
            content: "Calculate the result of (25 * 4) + (100 / 2) - 15",
            userId: "example_user_4");


        Console.WriteLine($"User: {prompt.Content}");


        var response = await _orchestrator.ProcessPromptAsync(prompt);


        Console.WriteLine($"Assistant: {response.Content}");

        if (response.ToolCallsExecuted.Any())
        {
            var calcTool = response.ToolCallsExecuted.First();
            Console.WriteLine($"Tool Used: {calcTool.ToolName}");
            Console.WriteLine($"Result: {calcTool.Result.Data}");
        }
    }


    /// <summary>
    /// Example 5: Error handling with invalid input.
    /// </summary>
    public async Task RunErrorHandlingExample()
    {
        Console.WriteLine("--- Example 5: Error Handling ---");

        var prompt = new Prompt(
            content: "Calculate the result of 10 / 0",
            userId: "example_user_5");


        Console.WriteLine($"User: {prompt.Content}");


        var response = await _orchestrator.ProcessPromptAsync(prompt);


        Console.WriteLine($"Assistant: {response.Content}");
        Console.WriteLine($"Status: {response.Status}");


        if (response.ToolCallsExecuted.Any())
        {
            var calcTool = response.ToolCallsExecuted.First();
            if (!calcTool.Result.Success)
            {
                Console.WriteLine($"Error: {calcTool.Result.ErrorMessage}");
            }
        }
    }


    /// <summary>
    /// Example 6: Retrieving and displaying metrics.
    /// </summary>
    public async Task RunMetricsExample()
    {
        Console.WriteLine("--- Example 6: Orchestration Metrics ---");

        var endTime = DateTime.UtcNow;
        var startTime = endTime.AddHours(-1);


        var metrics = await _monitor.GetMetricsAsync(startTime, endTime);


        Console.WriteLine($"Time Period: {startTime:HH:mm} - {endTime:HH:mm}");
        Console.WriteLine($"Total Prompts: {metrics.TotalPrompts}");
        Console.WriteLine($"Successful Responses: {metrics.SuccessfulResponses}");
        Console.WriteLine($"Failed Responses: {metrics.FailedResponses}");
        Console.WriteLine($"Average Response Time: {metrics.AverageResponseTime.TotalSeconds:F2}s");

        if (metrics.ToolUsageCount.Any())
        {
            Console.WriteLine("Tool Usage:");
            foreach (var tool in metrics.ToolUsageCount.OrderByDescending(t => t.Value))
            {
                Console.WriteLine($"  - {tool.Key}: {tool.Value} calls");
            }
        }
    }
}
