namespace Bam.AI.Orchestration.Agent
{
    /// <summary>
    /// An extracted entity from the prompt (dates, names, etc.).
    /// </summary>
    public interface IEntity
    {
        string Type { get; }
        string Value { get; }
        double Confidence { get; }
        int StartPosition { get; }
        int EndPosition { get; }
    }
}