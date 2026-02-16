namespace Bam.AI.Orchestration.Memory
{
    /// <summary>
    /// Scope of memory storage
    /// </summary>
    public enum MemoryScope
    {
        Session,      // Current conversation only
        User,         // Across all user conversations
        Global,       // Shared across all users
        Tool          // Tool-specific storage
    }
}