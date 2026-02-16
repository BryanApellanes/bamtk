using System.Collections.Generic;
using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.Model
{
    /// <summary>
    /// Request to the language model
    /// </summary>
    public interface IModelRequest
    {
        string SystemPrompt { get; }
        IEnumerable<IMessage> Messages { get; }
        IEnumerable<IToolDescriptor> AvailableTools { get; }
        IModelParameters Parameters { get; }
    }
}