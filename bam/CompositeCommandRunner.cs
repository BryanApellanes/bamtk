using Bam.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public class CompositeCommandRunner : IBrokeredCommandRunner
    {
        public IBrokeredCommandRunResult Run(IBrokeredCommand command, string[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
