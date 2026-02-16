using System;

namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// Capabilities a tool can provide
    /// </summary>
    [Flags]
    public enum ToolCapability
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
        Execute = 8,
        Search = 16,
        Transform = 32
    }
}