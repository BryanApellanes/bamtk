using System.Collections.Generic;

namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Capabilities of a language model
    /// </summary>
    public interface IModelCapabilities
    {
        int MaxContextTokens { get; }
        int MaxOutputTokens { get; }
        bool SupportsToolCalling { get; }
        bool SupportsVision { get; }
        bool SupportsStreaming { get; }
        IEnumerable<string> SupportedLanguages { get; }
    }
}