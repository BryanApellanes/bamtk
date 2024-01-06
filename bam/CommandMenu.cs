using Bam.Console;
using Bam.Net.CoreServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    [ConsoleMenu("code")]
    public class CommandMenu : ConsoleMenuContainer
    {
        public CommandMenu(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        public override ServiceRegistry Configure(ServiceRegistry serviceRegistry)
        {
            return serviceRegistry;
        }

        [ConsoleCommand("testWithStringParameters")]
        public void Test(string valueOne, string valueTwo)
        {
            Message.PrintLine("valueOne = {0}, valueTwo = {1}", valueOne, valueTwo);
        }

        [ConsoleCommand]
        public void SomeRandomName()
        {
            Message.PrintLine("This is some random method");
        }

        [ConsoleCommand("generate")]
        public void AnotherCommand()
        {
            Message.PrintLine("This is generate");
        }

        [ConsoleCommand("DEFAULT")]
        public void TheDefault()
        {
            Message.PrintLine("This is the DEFAULT");
        }

    }
}
