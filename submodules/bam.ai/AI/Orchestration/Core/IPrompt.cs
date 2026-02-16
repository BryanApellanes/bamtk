using System;
using System.Collections.Generic;

namespace Bam.AI.Orchestration.Core
{
    /// <summary>
    /// Represents a prompt or message from a user to the AI agent
    /// </summary>
    public interface IPrompt
    {
        string Content { get; }
        string UserId { get; }
        DateTime Timestamp { get; }
        IDictionary<string, object> Metadata { get; }
        IEnumerable<IAttachment> Attachments { get; }
    }
}