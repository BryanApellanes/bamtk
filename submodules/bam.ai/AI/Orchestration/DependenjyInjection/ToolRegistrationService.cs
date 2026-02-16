using Bam.AI.Orchestration.Tools;

namespace Bam.AI.Orchestration.DependencyInjection
{

    /// <summary>
    /// Background service that registers tools at application startup.
    /// </summary>
    internal class ToolRegistrationService : Microsoft.Extensions.Hosting.IHostedService
    {
        private readonly IToolRegistry _registry;
        private readonly IToolDescriptor _descriptor;


        public ToolRegistrationService(IToolRegistry registry, IToolDescriptor descriptor)
        {
            _registry = registry;
            _descriptor = descriptor;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _registry.RegisterTool(_descriptor);
            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}