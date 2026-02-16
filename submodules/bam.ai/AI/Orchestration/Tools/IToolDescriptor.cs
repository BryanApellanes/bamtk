namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// Describes a tool's capabilities and interface
    /// </summary>
    public interface IToolDescriptor
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        IToolSchema InputSchema { get; }
        IToolSchema OutputSchema { get; }
        ToolCapability Capabilities { get; }
        bool RequiresAuthorization { get; }
    }
}