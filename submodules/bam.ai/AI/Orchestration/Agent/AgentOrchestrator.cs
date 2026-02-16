using Microsoft.Extensions.Logging;
using Bam.AI.Orchestration.Core;
using Bam.AI.Orchestration.Model;
using Bam.AI.Orchestration.Safety;
using Bam.AI.Orchestration.Core.Models;
using Bam.AI.Orchestration.Monitoring;
using Bam.AI.Orchestration.Planning;
using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Agent
{
    /// <summary>
    /// Main orchestrator that coordinates the complete AI agent processing pipeline.
    /// </summary>
    public class AgentOrchestrator : IAgentOrchestrator
    {
        private readonly IPromptAnalyzer _promptAnalyzer;
        private readonly IToolRegistry _toolRegistry;
        private readonly IToolSelector _toolSelector;
        private readonly IToolExecutor _toolExecutor;
        private readonly ILanguageModel _languageModel;
        private readonly ITaskPlanner _taskPlanner;
        private readonly ISafetyValidator _safetyValidator;
        private readonly IOrchestrationMonitor _monitor;
        private readonly ILogger<AgentOrchestrator> _logger;

        public AgentOrchestrator(
            IPromptAnalyzer promptAnalyzer,
            IToolRegistry toolRegistry,
            IToolSelector toolSelector,
            IToolExecutor toolExecutor,
            ILanguageModel languageModel,
            ITaskPlanner taskPlanner,
            ISafetyValidator safetyValidator,
            IOrchestrationMonitor monitor,
            ILogger<AgentOrchestrator> logger)
        {
            _promptAnalyzer = promptAnalyzer ?? throw new ArgumentNullException(nameof(promptAnalyzer));
            _toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
            _toolSelector = toolSelector ?? throw new ArgumentNullException(nameof(toolSelector));
            _toolExecutor = toolExecutor ?? throw new ArgumentNullException(nameof(toolExecutor));
            _languageModel = languageModel ?? throw new ArgumentNullException(nameof(languageModel));
            _taskPlanner = taskPlanner ?? throw new ArgumentNullException(nameof(taskPlanner));
            _safetyValidator = safetyValidator ?? throw new ArgumentNullException(nameof(safetyValidator));
            _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IAgentResponse> ProcessPromptAsync(
            IPrompt prompt,
            CancellationToken cancellationToken = default)
        {
            if (prompt == null)
                throw new ArgumentNullException(nameof(prompt));

            var context = new ConversationContext($"conv_{Guid.NewGuid():N}");
            return await ProcessPromptWithContextAsync(prompt, context, cancellationToken);
        }

        public async Task<IAgentResponse> ProcessPromptWithContextAsync(
            IPrompt prompt,
            IConversationContext context,
            CancellationToken cancellationToken = default)
        {
            if (prompt == null)
                throw new ArgumentNullException(nameof(prompt));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var startTime = DateTime.UtcNow;
            _logger.LogInformation(
                "Processing prompt for user {UserId} in conversation {ConversationId}",
                prompt.UserId,
                context.ConversationId);

            _monitor.TrackPrompt(prompt);

            try
            {
                // Step 1: Safety validation
                var promptSafety = await _safetyValidator.ValidatePromptAsync(prompt, cancellationToken);
                if (!promptSafety.IsSafe)
                {
                    _logger.LogWarning(
                        "Prompt failed safety validation: {Violations}",
                        string.Join(", ", promptSafety.Violations.Select(v => v.Category)));

                    throw new SafetyViolationException(
                        "Prompt violates safety policies",
                        promptSafety.Violations);
                }

                // Step 2: Analyze prompt intent
                var intent = await _promptAnalyzer.AnalyzeAsync(prompt, context, cancellationToken);
                _logger.LogDebug(
                    "Analyzed intent: {IntentType} (confidence: {Confidence:P2})",
                    intent.Type,
                    intent.Confidence);

                // Step 3: Execute the appropriate processing path
                IAgentResponse response;
                if (intent.RequiresToolUse)
                {
                    response = await ProcessWithToolsAsync(prompt, intent, context, cancellationToken);
                }
                else
                {
                    response = await ProcessWithoutToolsAsync(prompt, intent, context, cancellationToken);
                }

                // Step 4: Validate response safety
                var responseSafety = await _safetyValidator.ValidateResponseAsync(response, cancellationToken);
                if (!responseSafety.IsSafe)
                {
                    _logger.LogWarning(
                        "Response failed safety validation: {Violations}",
                        string.Join(", ", responseSafety.Violations.Select(v => v.Category)));

                    response = new AgentResponse(
                        "I apologize, but I cannot provide that response as it may violate safety guidelines.",
                        ResponseStatus.Failed,
                        response.ToolCallsExecuted,
                        new Dictionary<string, object>
                        {
                            { "safety_violation", true },
                            { "violations", responseSafety.Violations.Select(v => v.Category).ToList() }
                        });
                }

                // Step 5: Update context and metrics
                context.AddExchange(prompt, response);
                var duration = DateTime.UtcNow - startTime;
                _monitor.TrackResponse(response, duration);

                _logger.LogInformation(
                    "Successfully processed prompt in {Duration:F2}s with status {Status}",
                    duration.TotalSeconds,
                    response.Status);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing prompt");
                _monitor.TrackError(ex, $"Prompt processing for user {prompt.UserId}");

                var duration = DateTime.UtcNow - startTime;
                var errorResponse = new AgentResponse(
                    "I apologize, but I encountered an error processing your request. Please try again.",
                    ResponseStatus.Failed,
                    metadata: new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "duration_seconds", duration.TotalSeconds }
                    });

                _monitor.TrackResponse(errorResponse, duration);
                return errorResponse;
            }
        }

        private async Task<IAgentResponse> ProcessWithToolsAsync(
            IPrompt prompt,
            IPromptIntent intent,
            IConversationContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Processing prompt with tools");

            var selectedTools = await _toolSelector.SelectToolsAsync(
                intent,
                _toolRegistry,
                cancellationToken);

            if (!selectedTools.Any())
            {
                _logger.LogWarning("Tool use required but no tools selected, falling back to direct processing");
                return await ProcessWithoutToolsAsync(prompt, intent, context, cancellationToken);
            }

            var availableToolDescriptors = selectedTools.Select(t => t.Descriptor);
            var plan = await _taskPlanner.CreatePlanAsync(
                intent,
                availableToolDescriptors,
                cancellationToken);

            _logger.LogDebug(
                "Created execution plan with {StepCount} steps",
                plan.Steps.Count());

            var toolResults = new List<IToolResult>();
            foreach (var step in plan.Steps.OrderBy(s => s.Order))
            {
                if (!ShouldExecuteStep(step, toolResults))
                {
                    _logger.LogDebug("Skipping step {Order} due to condition", step.Order);
                    continue;
                }

                var toolSafety = await _safetyValidator.ValidateToolCallAsync(
                    step.Tool,
                    cancellationToken);

                if (!toolSafety.IsSafe)
                {
                    _logger.LogWarning(
                        "Tool call {ToolName} failed safety validation",
                        step.Tool.Descriptor.Name);
                    continue;
                }

                _logger.LogDebug(
                    "Executing step {Order}: {ToolName}",
                    step.Order,
                    step.Tool.Descriptor.Name);

                var result = await _toolExecutor.ExecuteAsync(step.Tool, cancellationToken);
                toolResults.Add(result);

                var toolCall = new ToolCall(
                    step.Tool.Descriptor.Id,
                    step.Tool.Descriptor.Name,
                    step.Tool.Parameters,
                    result);

                _monitor.TrackToolExecution(toolCall);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Tool {ToolName} execution failed: {Error}",
                        step.Tool.Descriptor.Name,
                        result.ErrorMessage);
                }
            }

            var messages = BuildMessagesWithToolResults(context, prompt, toolResults);
            var modelRequest = new ModelRequest(
                systemPrompt: "You are a helpful AI assistant. Use the tool results to provide accurate, helpful responses.",
                messages: messages,
                availableTools: Enumerable.Empty<IToolDescriptor>(),
                parameters: new ModelParameters());

            var modelResponse = await _languageModel.GenerateAsync(modelRequest, cancellationToken);

            return new AgentResponse(
                modelResponse.Content,
                ResponseStatus.Success,
                toolResults.Select((r, i) => new ToolCall(
                    r.ToolId,
                    selectedTools.ElementAt(i).Descriptor.Name,
                    selectedTools.ElementAt(i).Parameters,
                    r)).ToList(),
                new Dictionary<string, object>
                {
                    { "plan_confidence", plan.ConfidenceScore },
                    { "tools_executed", toolResults.Count },
                    { "token_usage", modelResponse.Usage }
                });
        }

        private async Task<IAgentResponse> ProcessWithoutToolsAsync(
            IPrompt prompt,
            IPromptIntent intent,
            IConversationContext context,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Processing prompt without tools");

            var messages = BuildMessages(context, prompt);
            var modelRequest = new ModelRequest(
                systemPrompt: "You are a helpful AI assistant.",
                messages: messages,
                availableTools: Enumerable.Empty<IToolDescriptor>(),
                parameters: new ModelParameters());

            var modelResponse = await _languageModel.GenerateAsync(modelRequest, cancellationToken);

            return new AgentResponse(
                modelResponse.Content,
                ResponseStatus.Success,
                metadata: new Dictionary<string, object>
                {
                    { "intent_type", intent.Type.ToString() },
                    { "intent_confidence", intent.Confidence },
                    { "token_usage", modelResponse.Usage }
                });
        }

        private bool ShouldExecuteStep(IExecutionStep step, List<IToolResult> previousResults)
        {
            if (step.Condition == null)
                return true;

            switch (step.Condition.Type)
            {
                case ConditionType.Always:
                    return true;

                case ConditionType.IfSuccess:
                    return previousResults.LastOrDefault()?.Success ?? false;

                case ConditionType.IfFailure:
                    return !(previousResults.LastOrDefault()?.Success ?? true);

                case ConditionType.Custom:
                    return true;

                default:
                    return true;
            }
        }

        private IEnumerable<IMessage> BuildMessages(IConversationContext context, IPrompt prompt)
        {
            var messages = new List<IMessage>();

            var exchanges = context.PreviousPrompts
                .Zip(context.PreviousResponses, (p, r) => (Prompt: p, Response: r))
                .ToList();

            foreach (var (p, r) in exchanges)
            {
                messages.Add(new Message(MessageRole.User, p.Content));
                messages.Add(new Message(MessageRole.Assistant, r.Content));
            }

            messages.Add(new Message(MessageRole.User, prompt.Content));

            return messages;
        }

        private IEnumerable<IMessage> BuildMessagesWithToolResults(
            IConversationContext context,
            IPrompt prompt,
            List<IToolResult> toolResults)
        {
            var messages = BuildMessages(context, prompt).ToList();

            foreach (var result in toolResults)
            {
                var resultMessage = result.Success
                    ? $"Tool {result.ToolId} returned: {result.Data}"
                    : $"Tool {result.ToolId} failed: {result.ErrorMessage}";

                messages.Add(new Message(MessageRole.Tool, resultMessage));
            }

            return messages;
        }
    }
}