using System.Collections.Generic;
using Bam.AI.Orchestration.Agent;

namespace Bam.AI.Orchestration.Tools
{
    /// <summary>
    /// Registry of available tools the agent can use
    /// </summary>
    public interface IToolRegistry
    {
        IEnumerable<IToolDescriptor> GetAvailableTools();
        IToolDescriptor GetTool(string toolId);
        void RegisterTool(IToolDescriptor tool);
        void UnregisterTool(string toolId);
        IEnumerable<IToolDescriptor> FindToolsForIntent(IPromptIntent intent);
    }
}