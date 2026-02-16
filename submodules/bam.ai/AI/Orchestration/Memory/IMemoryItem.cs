using System;
using System.Collections.Generic;

namespace Bam.AI.Orchestration.Memory
{
    /// <summary>
    /// An item stored in memory
    /// </summary>
    public interface IMemoryItem
    {
        string Key { get; }
        object Value { get; }
        DateTime StoredAt { get; }
        DateTime? ExpiresAt { get; }
        IDictionary<string, string> Tags { get; }
    }
}