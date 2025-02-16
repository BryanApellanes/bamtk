using Bam.Command;
using Bam.Shell;

namespace Bam
{
    public class CompositeCommandBroker : CommandBroker
    {
        public CompositeCommandBroker(IMenuManager menuManager, MenuCommandRunner menuCommandRunner, ProcessCommandRunner processCommandRunner) : base(new CompositeCommandContextResolver(menuManager, menuCommandRunner, processCommandRunner))
        {
        }
    }
}
