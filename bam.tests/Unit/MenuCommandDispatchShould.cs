using Bam.Command;
using Bam.Console;
using Bam.DependencyInjection;
using Bam.Services;
using Bam.Shell;
using Bam.Test;

namespace Bam.Tests.Unit
{
    /// <summary>
    /// End-to-end brokered dispatch through the same wiring <c>bam/Program.Configure</c> builds —
    /// the test that would have caught the silent no-op dispatch of [ConsoleCommand]-only menu
    /// containers (BryanApellanes/threeheadz-tracker#17).
    /// </summary>
    [UnitTestMenu("Menu command dispatch should")]
    public class MenuCommandDispatchShould : UnitTestMenuContainer
    {
        public MenuCommandDispatchShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        [UnitTest(RunSynchronously = true)]
        public void DispatchConsoleCommandOnlyMenuCommand()
        {
            When.A<ICommandBroker>("brokers a [ConsoleCommand]-only menu command end to end",
                CreateCliBroker(),
                (broker) =>
                {
                    IBrokeredCommandResult result = broker.BrokerCommand(new string[] { "code", "another" });
                    return new DispatchOutcome(
                        result.Success,
                        result.RunResult != null,
                        result.RunResult?.Success ?? false);
                })
            .TheTest
            .ShouldPass<DispatchOutcome>((because, outcome) =>
            {
                because.ItsTrue("the brokered command succeeded", outcome.Success);
                because.ItsTrue("a run result was produced (the context and command resolved)", outcome.HasRunResult);
                because.ItsTrue("the run result reports success", outcome.RunResultSuccess);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void ReportUnresolvedContextVisibly()
        {
            string[] args = new string[] { "nope", "whatever" };

            When.A<ICommandBroker>("reports an unresolved context as a visible failure",
                CreateCliBroker(),
                (broker) =>
                {
                    IBrokeredCommandResult result = broker.BrokerCommand(args);
                    string failureMessage = BamCommandContext.GetFailureMessage(result, args);
                    return new UnresolvedOutcome(
                        result.Success,
                        !string.IsNullOrEmpty(failureMessage),
                        failureMessage.Contains("nope whatever"));
                })
            .TheTest
            .ShouldPass<UnresolvedOutcome>((because, outcome) =>
            {
                because.ItsFalse("the brokered command did not succeed", outcome.Success);
                because.ItsTrue("a failure message was produced instead of a silent null-deref", outcome.HasFailureMessage);
                because.ItsTrue("the failure message names the unresolved arguments", outcome.MessageNamesArguments);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        /// <summary>
        /// Builds the broker exactly as <c>bam/Program.Configure</c> does, pointing menu discovery at the
        /// CLI assembly (the test host's entry assembly is bam.tests, not bam).
        /// </summary>
        private static ICommandBroker CreateCliBroker()
        {
            MenuSpecs.LoadList = MenuSpecs.Scan(typeof(ConsoleCommands).Assembly);

            ServiceRegistry serviceRegistry = new ServiceRegistry();
            serviceRegistry.CombineWith(BamConsoleContext.Current.ServiceRegistry);
            serviceRegistry.CombineWith(BamCommandContext.Current.ServiceRegistry);

            serviceRegistry
                .For<ICommandArgumentParser>().Use<CommandArgumentParser>()
                .For<IBrokeredCommandArgumentProvider>().Use<SerializedFileBrokeredCommandArgumentProvider>()
                .For<ICommandBroker>().Use<CompositeCommandBroker>();

            return serviceRegistry.Get<ICommandBroker>();
        }

        private sealed record DispatchOutcome(bool Success, bool HasRunResult, bool RunResultSuccess);

        private sealed record UnresolvedOutcome(bool Success, bool HasFailureMessage, bool MessageNamesArguments);
    }
}
