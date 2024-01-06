using Bam.Command;
using Bam.Console;
using Bam.Net.CoreServices;
using Bam.Shell;

namespace Bam
{
    [Serializable]
    class Program
    {
        static void Main(string[] args)
        {
            Configure();
            BamCommandContext.Main(args);
        }

        static void Configure()
        {
            ServiceRegistry serviceRegistry = new ServiceRegistry();
            serviceRegistry.CombineWith(BamConsoleContext.Current.ServiceRegistry);
            serviceRegistry.CombineWith(BamCommandContext.Current.ServiceRegistry);

            serviceRegistry
                .For<ICommandArgumentParser>().Use<CommandArgumentParser>()
                .For<IBrokeredCommandArgumentProvider>().Use<SerializedFileBrokeredCommandArgumentProvider>()
                .For<ICommandBroker>().Use<CompositeCommandBroker>();

            BamCommandContext.Current.ServiceRegistry.CombineWith(serviceRegistry);
        }
    }
}
