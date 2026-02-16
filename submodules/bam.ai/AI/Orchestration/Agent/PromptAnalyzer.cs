using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bam.AI.Orchestration.Core;
using Microsoft.Extensions.Logging;

namespace Bam.AI.Orchestration.Agent
{
    /// <summary>
    /// Analyzes user prompts to extract intent, entities, and determine tool requirements.
    /// </summary>
    public class PromptAnalyzer : IPromptAnalyzer
    {
        private readonly ILogger<PromptAnalyzer> _logger;

        private static readonly Dictionary<IntentType, List<string>> IntentKeywords = new()
        {
            [IntentType.Question] = new() { "what", "who", "when", "where", "why", "how", "is", "are", "can", "does", "?" },
            [IntentType.Command] = new() { "create", "make", "build", "generate", "delete", "remove", "update", "modify", "send" },
            [IntentType.CreativeRequest] = new() { "write", "compose", "design", "draft", "imagine", "invent", "creative" },
            [IntentType.DataRetrieval] = new() { "find", "search", "lookup", "get", "retrieve", "fetch", "show", "list", "query" },
            [IntentType.Computation] = new() { "calculate", "compute", "sum", "count", "analyze", "process", "transform" },
            [IntentType.Analysis] = new() { "analyze", "compare", "evaluate", "assess", "review", "examine", "study" },
            [IntentType.Conversation] = new() { "hello", "hi", "thanks", "thank you", "bye", "goodbye", "chat" }
        };

        private static readonly HashSet<string> ToolUseIndicators = new(StringComparer.OrdinalIgnoreCase)
        {
            "search", "find", "lookup", "database", "file", "api", "web", "current", "latest",
            "real-time", "live", "now", "today", "fetch", "retrieve", "get", "check",
            "calculate", "compute", "process", "transform", "convert", "generate code",
            "run", "execute", "call", "invoke", "create file", "save", "send email"
        };

        public PromptAnalyzer(ILogger<PromptAnalyzer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<IPromptIntent> AnalyzeAsync(
            IPrompt prompt,
            IConversationContext context,
            CancellationToken cancellationToken = default)
        {
            if (prompt == null)
                throw new ArgumentNullException(nameof(prompt));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _logger.LogDebug("Analyzing prompt: {Content}", 
                prompt.Content.Substring(0, Math.Min(100, prompt.Content.Length)));

            var normalizedContent = prompt.Content.ToLowerInvariant();
            var words = TokenizePrompt(normalizedContent);

            var entities = ExtractEntities(prompt.Content);
            var intentScores = CalculateIntentScores(normalizedContent, words);
            var primaryIntent = intentScores.OrderByDescending(kvp => kvp.Value).First();
            var requiresToolUse = DetermineToolRequirement(normalizedContent, words, primaryIntent.Key, entities);

            var intent = new PromptIntent(
                type: primaryIntent.Key,
                confidence: primaryIntent.Value,
                extractedEntities: entities,
                requiresToolUse: requiresToolUse,
                summary: GenerateSummary(primaryIntent.Key, entities));

            _logger.LogDebug(
                "Analyzed intent: {Type} (confidence: {Confidence:P2}, requires tools: {RequiresTools})",
                intent.Type,
                intent.Confidence,
                intent.RequiresToolUse);

            return Task.FromResult<IPromptIntent>(intent);
        }

        private List<string> TokenizePrompt(string text)
        {
            var cleaned = Regex.Replace(text, @"[^\w\s?!-]", " ");

            return cleaned
                .Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.ToLowerInvariant())
                .ToList();
        }

        private List<IEntity> ExtractEntities(string text)
        {
            var entities = new List<IEntity>();

            var datePattern = @"\b(\d{4}-\d{2}-\d{2}|\d{1,2}/\d{1,2}/\d{2,4}|today|tomorrow|yesterday)\b";
            var dateMatches = Regex.Matches(text, datePattern, RegexOptions.IgnoreCase);
            foreach (Match match in dateMatches)
            {
                entities.Add(new Entity(
                    type: "date",
                    value: match.Value,
                    confidence: 0.95,
                    startPosition: match.Index,
                    endPosition: match.Index + match.Length));
            }

            var timePattern = @"\b(\d{1,2}:\d{2}(?:\s?[ap]m)?|\d{1,2}\s?[ap]m)\b";
            var timeMatches = Regex.Matches(text, timePattern, RegexOptions.IgnoreCase);
            foreach (Match match in timeMatches)
            {
                entities.Add(new Entity(
                    type: "time",
                    value: match.Value,
                    confidence: 0.9,
                    startPosition: match.Index,
                    endPosition: match.Index + match.Length));
            }

            var numberPattern = @"\b\d+(?:\.\d+)?\b";
            var numberMatches = Regex.Matches(text, numberPattern);
            foreach (Match match in numberMatches)
            {
                entities.Add(new Entity(
                    type: "number",
                    value: match.Value,
                    confidence: 0.99,
                    startPosition: match.Index,
                    endPosition: match.Index + match.Length));
            }

            var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
            var emailMatches = Regex.Matches(text, emailPattern);
            foreach (Match match in emailMatches)
            {
                entities.Add(new Entity(
                    type: "email",
                    value: match.Value,
                    confidence: 0.95,
                    startPosition: match.Index,
                    endPosition: match.Index + match.Length));
            }

            var urlPattern = @"https?://[^\s]+";
            var urlMatches = Regex.Matches(text, urlPattern, RegexOptions.IgnoreCase);
            foreach (Match match in urlMatches)
            {
                entities.Add(new Entity(
                    type: "url",
                    value: match.Value,
                    confidence: 0.99,
                    startPosition: match.Index,
                    endPosition: match.Index + match.Length));
            }

            var filePathPattern = @"(?:[A-Za-z]:\\|/)(?:[^\\\s]+\\)*[^\\\s]+\.\w+";
            var filePathMatches = Regex.Matches(text, filePathPattern);
            foreach (Match match in filePathMatches)
            {
                entities.Add(new Entity(
                    type: "filepath",
                    value: match.Value,
                    confidence: 0.85,
                    startPosition: match.Index,
                    endPosition: match.Index + match.Length));
            }

            return entities;
        }

        private Dictionary<IntentType, double> CalculateIntentScores(
            string normalizedContent,
            List<string> words)
        {
            var scores = new Dictionary<IntentType, double>();

            foreach (var intentType in Enum.GetValues<IntentType>())
            {
                if (!IntentKeywords.ContainsKey(intentType))
                {
                    scores[intentType] = 0.0;
                    continue;
                }

                var keywords = IntentKeywords[intentType];
                var matchCount = words.Count(w => keywords.Contains(w));
                var matchScore = (double)matchCount / Math.Max(words.Count, 1);

                var earlyMatchBonus = 0.0;
                var firstFiveWords = words.Take(5);
                if (firstFiveWords.Any(w => keywords.Contains(w)))
                {
                    earlyMatchBonus = 0.2;
                }

                var phraseMatchBonus = 0.0;
                foreach (var keyword in keywords)
                {
                    if (normalizedContent.Contains(keyword))
                    {
                        phraseMatchBonus += 0.1;
                    }
                }

                scores[intentType] = Math.Min(1.0, matchScore + earlyMatchBonus + phraseMatchBonus);
            }

            var totalScore = scores.Values.Sum();
            if (totalScore > 0)
            {
                foreach (var key in scores.Keys.ToList())
                {
                    scores[key] /= totalScore;
                }
            }
            else
            {
                scores[IntentType.Conversation] = 1.0;
            }

            return scores;
        }

        private bool DetermineToolRequirement(
            string normalizedContent,
            List<string> words,
            IntentType primaryIntent,
            List<IEntity> entities)
        {
            if (words.Any(w => ToolUseIndicators.Contains(w)))
                return true;

            if (entities.Any(e => e.Type == "url" || e.Type == "filepath" || e.Type == "email"))
                return true;

            switch (primaryIntent)
            {
                case IntentType.DataRetrieval:
                case IntentType.Computation:
                    return true;

                case IntentType.Command:
                    return words.Any(w => new[] { "create", "delete", "send", "save", "update" }.Contains(w));

                case IntentType.Analysis:
                    return entities.Any(e => e.Type == "filepath" || e.Type == "url");

                case IntentType.Question:
                    return words.Any(w => new[] { "current", "latest", "today", "now", "real-time" }.Contains(w));

                case IntentType.CreativeRequest:
                case IntentType.Conversation:
                default:
                    return false;
            }
        }

        private string GenerateSummary(IntentType intentType, List<IEntity> entities)
        {
            var summary = intentType switch
            {
                IntentType.Question => "User is asking a question",
                IntentType.Command => "User is requesting an action",
                IntentType.CreativeRequest => "User wants creative content",
                IntentType.DataRetrieval => "User needs to retrieve data",
                IntentType.Computation => "User requires a calculation",
                IntentType.Analysis => "User wants analysis",
                IntentType.Conversation => "User is engaging in conversation",
                _ => "General user request"
            };

            if (entities.Any())
            {
                var entityTypes = entities
                    .Select(e => e.Type)
                    .Distinct()
                    .ToList();
                summary += $" involving {string.Join(", ", entityTypes)}";
            }

            return summary;
        }
    }
}