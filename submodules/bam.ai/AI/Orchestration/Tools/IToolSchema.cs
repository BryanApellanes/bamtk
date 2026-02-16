using System.Collections.Generic;

namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// Schema definition for tool inputs/outputs
    /// </summary>
    public interface IToolSchema
    {
        string SchemaType { get; } // JSON Schema, XML Schema, etc.
        string SchemaDefinition { get; }
        IEnumerable<IParameter> Parameters { get; }
    }
}