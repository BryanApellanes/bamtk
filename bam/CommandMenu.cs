using Bam.Console;
using Bam.Net.CoreServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public class CommandMenu : ConsoleMenuContainer
    {
        public CommandMenu(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        public override ServiceRegistry Configure(ServiceRegistry serviceRegistry)
        {
            return serviceRegistry;
        }

        [ConsoleCommand]
        public void Default()
        {
            Message.PrintLine("This is the default method");
        }
    }
}
