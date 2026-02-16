namespace Bam.AI.Orchestration.Safety
{
    /// <summary>
    /// A safety violation
    /// </summary>
    public interface ISafetyViolation
    {
        string Type { get; }
        string Description { get; }
        string Category { get; }
        SafetyLevel Severity { get; }
    }
}