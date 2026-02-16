# bam.ai

An AI agent orchestration framework providing a complete pipeline for prompt analysis, tool selection, execution planning, safety validation, and language model interaction.

## Overview

bam.ai implements a modular AI agent orchestration system built on Microsoft.Extensions.DependencyInjection. The central component, `AgentOrchestrator`, coordinates a multi-step processing pipeline: (1) validate prompt safety, (2) analyze prompt intent, (3) select and execute tools if needed, (4) generate a response via a language model, and (5) validate response safety. All steps are asynchronous and support cancellation tokens.

The framework includes abstractions for multiple LLM providers (Anthropic Claude, OpenAI, Azure OpenAI, and local models) through the `ILanguageModel` interface. A tool system allows registering, discovering, and executing tools at runtime via `IToolRegistry` and `IToolExecutor`. The `TaskPlanner` creates execution plans that order tools by capability priority (Read/Search, then Transform, then Write/Execute) with dependency tracking and conditional execution.

Safety is handled by `SafetyValidator`, which performs regex-based content scanning for injection attacks, prompt injection attempts, PII (SSN, credit card patterns), and harmful content. A monitoring subsystem (`OrchestrationMonitor`) tracks prompts, responses, tool executions, and errors with metrics aggregation. Memory management through `MemoryManager` supports scoped memory items for conversation context.

## Key Classes

| Class | Description |
|---|---|
| `AgentOrchestrator` | Main orchestrator coordinating the full AI processing pipeline |
| `IAgentOrchestrator` | Interface for the orchestrator: `ProcessPromptAsync` and `ProcessPromptWithContextAsync` |
| `PromptAnalyzer` | Analyzes prompts to determine intent type, confidence, and required tool use |
| `ConversationContext` | Maintains conversation history (previous prompts and responses) |
| `AnthropicLanguageModel` | Anthropic Claude API integration with tool calling support |
| `OpenAILanguageModel` | OpenAI API integration |
| `AzureOpenAILanguageModel` | Azure OpenAI API integration |
| `LocalLanguageModel` | Local/self-hosted model integration |
| `LanguageModelFactory` | Creates language model instances based on configuration |
| `ToolRegistry` | Thread-safe concurrent registry for available tools |
| `ToolSelector` | Selects appropriate tools based on prompt intent |
| `ToolExecutor` | Executes selected tools and collects results |
| `ToolDescriptor` | Metadata describing a tool: ID, name, capabilities, schemas |
| `TaskPlanner` | Creates ordered execution plans from intent and available tools |
| `ExecutionPlan` / `ExecutionStep` | Represents a plan with ordered steps, conditions, and dependencies |
| `SafetyValidator` | Validates prompts, responses, and tool calls for safety violations |
| `OrchestrationMonitor` / `OrchestrationMetrics` | Tracks pipeline metrics: prompt counts, response times, tool usage |
| `MemoryManager` / `MemoryItem` | Scoped memory system for conversation context |
| `ServiceCollectionExtensions` | DI registration: `AddAIOrchestration`, `AddTool<T>`, `AddToolDescriptor` |
| `ExampleOrchestrationService` | Example scenarios: simple questions, tool use, multi-turn conversations |

## Dependencies

**Package References:**
- `Microsoft.Extensions.Configuration` 10.0.2
- `Microsoft.Extensions.Hosting` 10.0.2
- `Microsoft.Extensions.Logging` 10.0.2

**Target Framework:** net10.0

## Usage Examples

```csharp
using Bam.AI.Orchestration.DependencyInjection;
using Bam.AI.Orchestration.Agent;
using Bam.AI.Orchestration.Core.Models;

// Register services in DI
services.AddAIOrchestration(configuration);

// Or with a custom language model
services.AddAIOrchestration(sp =>
    new AnthropicLanguageModel(apiKey, "claude-sonnet-4-20250514", endpoint, timeout, logger));

// Register tools
services.AddTool<WeatherApiTool>("weather_api");
services.AddToolDescriptor(new ToolDescriptor(
    id: "weather_api",
    name: "Weather API",
    description: "Gets current weather for a location",
    inputSchema: weatherSchema,
    outputSchema: weatherOutputSchema,
    capabilities: ToolCapability.Read | ToolCapability.Search));

// Process a prompt
var orchestrator = serviceProvider.GetRequiredService<IAgentOrchestrator>();
var prompt = new Prompt(content: "What's the weather in Seattle?", userId: "user_1");
var response = await orchestrator.ProcessPromptAsync(prompt);

Console.WriteLine(response.Content);
Console.WriteLine($"Tools used: {response.ToolCallsExecuted.Count()}");

// Multi-turn conversation
var context = new ConversationContext("conversation_1");
var response1 = await orchestrator.ProcessPromptWithContextAsync(prompt1, context);
var response2 = await orchestrator.ProcessPromptWithContextAsync(prompt2, context);
```

## Known Gaps / Not Yet Implemented

- **Streaming is not fully implemented** in `AnthropicLanguageModel.GenerateStreamingAsync` -- it falls back to non-streaming and reports the full response at once. SSE parsing is not implemented.
- `OpenAILanguageModel`, `AzureOpenAILanguageModel`, and `LocalLanguageModel` exist as classes but their full implementations should be verified.
- The `SafetyValidator` uses basic regex patterns; the code notes that production use should integrate dedicated content safety APIs (Azure Content Safety, OpenAI Moderation, Perspective API).
- The `TaskPlanner` uses simplified linear dependency chains; more sophisticated data-flow analysis is noted as future work.
- The `ToolSelector` and `PromptAnalyzer` use heuristic-based approaches rather than ML-based classification.
- There is a typo in the namespace: `DependenjyInjection` (used for `ToolRegistrationService`), though the public `ServiceCollectionExtensions` class uses the correct `DependencyInjection` namespace.
