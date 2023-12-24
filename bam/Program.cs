using Bam.Command;
using Bam.Console;

namespace Bam
{
    [Serializable]
    class Program
    {
        static void Main(string[] args)
        {
            BamCommandContext.Current.Configure(svcRegistry =>
            {
                svcRegistry.For<ICommandBroker>().Use<ProcessCommandBroker>();
            });
            BamCommandContext.Main(args);
        }
    }
}
