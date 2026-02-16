using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bam.AI.Orchestration.Core;
using Bam.AI.Orchestration.Tools;


namespace Bam.AI.Orchestration.Safety
{
    /// <summary>
    /// Validates prompts, responses, and tool calls for safety violations.
    /// </summary>
    /// <remarks>
    /// The safety validator implements content moderation and security checks to
    /// prevent:
    /// - Harmful content generation
    /// - Security vulnerabilities (injection attacks, data leaks)
    /// - Unauthorized operations
    /// - Privacy violations
    /// - Abuse and misuse
    /// 
    /// This implementation provides basic rule-based validation. For production use,
    /// consider integrating:
    /// - Azure Content Safety API
    /// - OpenAI Moderation API
    /// - Perspective API (Google)
    /// - Custom ML-based classifiers
    /// 
    /// Thread Safety: This class is thread-safe and stateless.
    /// </remarks>
    public class SafetyValidator : ISafetyValidator
    {
        private readonly ILogger<SafetyValidator> _logger;


        // Patterns for detecting potentially harmful content
        private static readonly List<(Regex Pattern, string Category, SafetyLevel Severity)> HarmfulPatterns = new()
        {
            // Injection attacks
            (new Regex(@"(?:union\s+select|drop\s+table|exec\s*\(|<script|javascript:)", RegexOptions.IgnoreCase),
             "Injection Attack", SafetyLevel.Critical),
            
            // Prompt injection attempts
            (new Regex(@"ignore\s+(previous|above|all)\s+(instructions|prompts|rules)", RegexOptions.IgnoreCase),
             "Prompt Injection", SafetyLevel.HighRisk),
            
            // Personally identifiable information (PII)
            (new Regex(@"\b\d{3}-\d{2}-\d{4}\b"), // SSN pattern
             "PII - SSN", SafetyLevel.MediumRisk),
            (new Regex(@"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b"), // Credit card pattern
             "PII - Credit Card", SafetyLevel.HighRisk),
            
            // Potentially harmful instructions
            (new Regex(@"(how\s+to\s+(hack|crack|exploit|bypass|steal))", RegexOptions.IgnoreCase),
             "Harmful Instructions", SafetyLevel.MediumRisk),
            
            // Hate speech indicators (simplified)
            (new Regex(@"\b(hate|kill|attack)\s+(all|every)\s+\w+s\b", RegexOptions.IgnoreCase),
             "Hate Speech", SafetyLevel.HighRisk),
        };


        // Sensitive operations that require extra validation
        private static readonly HashSet<string> SensitiveOperations = new(StringComparer.OrdinalIgnoreCase)
        {
            "delete", "remove", "drop", "truncate", "destroy", "purge", "erase",
            "modify", "update", "change", "alter", "edit",
            "send", "email", "post", "publish", "share", "upload"
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="SafetyValidator"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for diagnostics.</param>
        public SafetyValidator(ILogger<SafetyValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Validates a prompt for safety violations.
        /// </summary>
        /// <param name="prompt">The prompt to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the validation result with any detected violations.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="prompt"/> is null.
        /// </exception>
        /// <remarks>
        /// Validation checks include:
        /// - Content scanning for harmful patterns
        /// - PII detection
        /// - Injection attack detection
        /// - Length limits
        /// - Rate limiting (not implemented here, but recommended)
        /// 
        /// The validator is intentionally permissive to avoid false positives.
        /// It flags high-confidence violations as unsafe and logs medium-confidence
        /// issues for review.
        /// </remarks>
        public Task<ISafetyResult> ValidatePromptAsync(
            IPrompt prompt,
            CancellationToken cancellationToken = default)
        {
            if (prompt == null)
                throw new ArgumentNullException(nameof(prompt));


            _logger.LogDebug("Validating prompt for user {UserId}", prompt.UserId);


            var violations = new List<ISafetyViolation>();


            // Check for harmful patterns
            foreach (var (pattern, category, severity) in HarmfulPatterns)
            {
                if (pattern.IsMatch(prompt.Content))
                {
                    violations.Add(new SafetyViolation(
                        category: category,
                        description: $"Content matched pattern for {category}",
                        confidence: 0.8,
                        severity: severity));


                    _logger.LogWarning(
                        "Detected potential {Category} in prompt from user {UserId}",
                        category,
                        prompt.UserId);
                }
            }


            // Check prompt length (very long prompts might indicate abuse)
            if (prompt.Content.Length > 50000)
            {
                violations.Add(new SafetyViolation(
                    category: "Excessive Length",
                    description: $"Prompt length ({prompt.Content.Length} chars) exceeds reasonable limits",
                    confidence: 0.9,
                    severity: SafetyLevel.LowRisk));
            }


            // Check attachments
            foreach (var attachment in prompt.Attachments)
            {
                if (attachment.Size > 100 * 1024 * 1024) // 100MB
                {
                    violations.Add(new SafetyViolation(
                        category: "Excessive File Size",
                        description: $"Attachment {attachment.Name} is too large ({attachment.Size} bytes)",
                        confidence: 1.0,
                        severity: SafetyLevel.MediumRisk));
                }
            }


            var highestSeverity = violations.Any()
                ? violations.Max(v => v.Severity)
                : SafetyLevel.Safe;


            var isSafe = highestSeverity < SafetyLevel.HighRisk;


            var result = new SafetyResult(
                isSafe: isSafe,
                level: highestSeverity,
                violations: violations,
                recommendedAction: GetRecommendedAction(highestSeverity));


            if (!isSafe)
            {
                _logger.LogWarning(
                    "Prompt validation failed with {ViolationCount} violations at level {Level}",
                    violations.Count,
                    highestSeverity);
            }


            return Task.FromResult<ISafetyResult>(result);
        }


        /// <summary>
        /// Validates a response for safety violations.
        /// </summary>
        /// <param name="response">The response to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the validation result with any detected violations.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="response"/> is null.
        /// </exception>
        /// <remarks>
        /// Response validation checks for:
        /// - Harmful content in the generated text
        /// - PII leakage
        /// - Inappropriate or offensive content
        /// - Security-sensitive information disclosure
        /// 
        /// This helps ensure the AI doesn't generate problematic content even if
        /// the prompt was benign.
        /// </remarks>
        public Task<ISafetyResult> ValidateResponseAsync(
            IAgentResponse response,
            CancellationToken cancellationToken = default)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));


            _logger.LogDebug("Validating agent response");


            var violations = new List<ISafetyViolation>();


            // Check for harmful patterns in response
            foreach (var (pattern, category, severity) in HarmfulPatterns)
            {
                if (pattern.IsMatch(response.Content))
                {
                    violations.Add(new SafetyViolation(
                        category: category,
                        description: $"Response contained {category}",
                        confidence: 0.7, // Slightly lower confidence for responses
                        severity: severity));


                    _logger.LogWarning(
                        "Detected potential {Category} in response",
                        category);
                }
            }


            // Check for potential data leakage
            if (ContainsSensitiveData(response.Content))
            {
                violations.Add(new SafetyViolation(
                    category: "Potential Data Leakage",
                    description: "Response may contain sensitive information",
                    confidence: 0.6,
                    severity: SafetyLevel.MediumRisk));
            }


            var highestSeverity = violations.Any()
                ? violations.Max(v => v.Severity)
                : SafetyLevel.Safe;


            var isSafe = highestSeverity < SafetyLevel.HighRisk;


            var result = new SafetyResult(
                isSafe: isSafe,
                level: highestSeverity,
                violations: violations,
                recommendedAction: GetRecommendedAction(highestSeverity));


            return Task.FromResult<ISafetyResult>(result);
        }


        /// <summary>
        /// Validates a tool call for safety violations.
        /// </summary>
        /// <param name="tool">The tool to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the validation result with any detected violations.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tool"/> is null.
        /// </exception>
        /// <remarks>
        /// Tool call validation is critical for security. It checks:
        /// - Whether the tool requires authorization
        /// - Whether parameters contain malicious content
        /// - Whether the operation is potentially destructive
        /// - Whether the tool is being used appropriately
        /// 
        /// Destructive operations (delete, modify) get extra scrutiny.
        /// </remarks>
        public Task<ISafetyResult> ValidateToolCallAsync(
            ISelectedTool tool,
            CancellationToken cancellationToken = default)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));


            _logger.LogDebug("Validating tool call for {ToolName}", tool.Descriptor.Name);


            var violations = new List<ISafetyViolation>();


            // Check if tool requires authorization
            if (tool.Descriptor.RequiresAuthorization)
            {
                // In a real implementation, verify user has granted permission
                // For now, we'll just flag it
                _logger.LogInformation(
                    "Tool {ToolName} requires authorization",
                    tool.Descriptor.Name);
            }


            // Check for sensitive operations
            var hasSensitiveOperation = SensitiveOperations.Any(op =>
                tool.Descriptor.Name.Contains(op, StringComparison.OrdinalIgnoreCase) ||
                tool.Descriptor.Description.Contains(op, StringComparison.OrdinalIgnoreCase));


            if (hasSensitiveOperation)
            {
                _logger.LogInformation(
                    "Tool {ToolName} performs sensitive operation",
                    tool.Descriptor.Name);


                // Check if this is a destructive operation with insufficient confirmation
                if (tool.Descriptor.Capabilities.HasFlag(ToolCapability.Delete) ||
                    tool.Descriptor.Capabilities.HasFlag(ToolCapability.Write))
                {
                    violations.Add(new SafetyViolation(
                        category: "Sensitive Operation",
                        description: $"Tool {tool.Descriptor.Name} performs potentially destructive operations",
                        confidence: 0.8,
                        severity: SafetyLevel.MediumRisk));
                }
            }


            // Check parameters for malicious content
            foreach (var param in tool.Parameters)
            {
                var paramValue = param.Value?.ToString() ?? string.Empty;


                foreach (var (pattern, category, severity) in HarmfulPatterns)
                {
                    if (pattern.IsMatch(paramValue))
                    {
                        violations.Add(new SafetyViolation(
                            category: $"Malicious Parameter - {category}",
                            description: $"Parameter '{param.Key}' contains potentially malicious content",
                            confidence: 0.9,
                            severity: severity));


                        _logger.LogWarning(
                            "Detected {Category} in parameter {ParamName} of tool {ToolName}",
                            category,
                            param.Key,
                            tool.Descriptor.Name);
                    }
                }
            }


            var highestSeverity = violations.Any()
                ? violations.Max(v => v.Severity)
                : SafetyLevel.Safe;


            var isSafe = highestSeverity < SafetyLevel.HighRisk;


            var result = new SafetyResult(
                isSafe: isSafe,
                level: highestSeverity,
                violations: violations,
                recommendedAction: GetRecommendedAction(highestSeverity));


            return Task.FromResult<ISafetyResult>(result);
        }


        /// <summary>
        /// Checks if content contains sensitive data.
        /// </summary>
        /// <remarks>
        /// This is a simplified implementation. Production systems should use
        /// more sophisticated PII detection, such as:
        /// - Named Entity Recognition (NER) models
        /// - Azure Cognitive Services for PII detection
        /// - Custom ML models trained on your data
        /// </remarks>
        private bool ContainsSensitiveData(string content)
        {
            // Check for common PII patterns
            var piiPatterns = new[]
            {
                @"\b\d{3}-\d{2}-\d{4}\b", // SSN
                @"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b", // Credit card
                @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", // Email (might be okay in some contexts)
            };


            foreach (var pattern in piiPatterns)
            {
                if (Regex.IsMatch(content, pattern))
                    return true;
            }


            return false;
        }


        /// <summary>
        /// Gets a recommended action based on severity level.
        /// </summary>
        private string GetRecommendedAction(SafetyLevel level)
        {
            return level switch
            {
                SafetyLevel.Safe => "Proceed normally",
                SafetyLevel.LowRisk => "Proceed with logging",
                SafetyLevel.MediumRisk => "Proceed with user confirmation",
                SafetyLevel.HighRisk => "Block and notify user",
                SafetyLevel.Critical => "Block immediately and alert security team",
                _ => "Review manually"
            };
        }
    }
}


