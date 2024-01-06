using Bam.Command;
using Bam.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public class CompositeCommandBroker : CommandBroker
    {
        public CompositeCommandBroker(IMenuManager menuManager, MenuCommandRunner menuCommandRunner, ProcessCommandRunner processCommandRunner) : base(new CompositeCommandContextResolver(menuManager, menuCommandRunner, processCommandRunner))
        {
        }
    }
}
