using System.Collections.Generic;

namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// A parameter in a tool schema
    /// </summary>
    public interface IParameter
    {
        string Name { get; }
        string Type { get; }
        bool Required { get; }
        object DefaultValue { get; }
        string Description { get; }
        IEnumerable<string> AllowedValues { get; }
    }
}