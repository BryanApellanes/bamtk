namespace Bam.AI.Orchestration.Safety
{
    /// <summary>
    /// Safety level classification
    /// </summary>
    public enum SafetyLevel
    {
        Safe,
        Warning,
        LowRisk,
        MediumRisk,
        HighRisk,
        Violation,
        Critical
    }
}