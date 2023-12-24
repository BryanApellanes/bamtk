using Bam.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public class ProcessCommandBroker : CommandBroker
    {
        public ProcessCommandBroker() : base(new ProcessCommandContextResolver())
        {
        }
    }
}
