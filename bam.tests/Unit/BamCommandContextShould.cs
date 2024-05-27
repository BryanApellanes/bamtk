using Bam.Command;
using Bam.Console;
using Bam;
using Bam.CoreServices;
using Bam.Testing;
using Bam.Tests.TestClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Tests.Unit
{
    [UnitTestMenu("BamContext should")]
    public class BamCommandContextShould : UnitTestMenuContainer
    {
        public BamCommandContextShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        [UnitTest(RunSynchronously = true)]
        public void UseConfiguredContextResolverGenericType()
        {
            ServiceRegistry svcRegistry = BamCommandContext.Current.ServiceRegistry;

            IBrokeredCommandContextResolver commandContextResolver = svcRegistry.Get<IBrokeredCommandContextResolver>();

            // The default type is ProcessCommandContextResolver set by BamCommandContext.GetServiceRegitry()
            commandContextResolver.ShouldBeOfType<ProcessCommandContextResolver>();

            svcRegistry.For<IBrokeredCommandContextResolver>().Use<TestCommandContextResolver>();

            commandContextResolver = svcRegistry.Get<IBrokeredCommandContextResolver>();

            commandContextResolver.ShouldBeOfType<TestCommandContextResolver>();

            BamCommandContext.Current.ServiceRegistry.ShouldBe(svcRegistry);

            commandContextResolver = BamCommandContext.Current.ServiceRegistry
                .Get<IBrokeredCommandContextResolver>();
            
            commandContextResolver.ShouldBeOfType<TestCommandContextResolver>($"Should have been of type {nameof(TestCommandContextResolver)} but was {commandContextResolver.GetType().Name}");
        }

        [UnitTest(RunSynchronously = true)]
        public void UseConfiguredContextResolverInstanciator()
        {
            ServiceRegistry svcRegistry = BamCommandContext.Current.ServiceRegistry;

            IBrokeredCommandContextResolver commandContextResolver = svcRegistry.Get<IBrokeredCommandContextResolver>();

            // The default type is ProcessCommandContextResolver set by BamCommandContext.GetServiceRegistry()
            commandContextResolver.ShouldBeOfType<ProcessCommandContextResolver>();

            svcRegistry.For<IBrokeredCommandContextResolver>().Use(() => new TestCommandContextResolver());

            commandContextResolver = svcRegistry.Get<IBrokeredCommandContextResolver>();
            commandContextResolver.ShouldBeOfType<TestCommandContextResolver>();

            BamCommandContext.Current.ServiceRegistry.ShouldBe(svcRegistry);

            // Make sure the static singleton returns expected instance
            commandContextResolver = BamCommandContext.Current.ServiceRegistry
                .Get<IBrokeredCommandContextResolver>();
            commandContextResolver.ShouldBeOfType<TestCommandContextResolver>($"Should have been of type {nameof(TestCommandContextResolver)} but was {commandContextResolver.GetType().Name}");
        }

        [UnitTest(RunSynchronously = true)]
        public void UseConfiguredContextResolverParameterizedInstanciator()
        {
            ServiceRegistry svcRegistry = BamCommandContext.Current.ServiceRegistry;

            IBrokeredCommandContextResolver commandContextResolver = svcRegistry.Get<IBrokeredCommandContextResolver>();

            // The default type is ProcessCommandContextResolver set by BamCommandContext.GetServiceRegitry()
            commandContextResolver.ShouldBeOfType<ProcessCommandContextResolver>();

            svcRegistry.For<IBrokeredCommandContextResolver>().Use((svcReg) => new TestCommandContextResolver(svcReg.Get<IBamBrokeredCommandContext>()));

            commandContextResolver = svcRegistry.Get<IBrokeredCommandContextResolver>();

            commandContextResolver.ShouldBeOfType<TestCommandContextResolver>();
            TestCommandContextResolver testCommandContextResolver = ((TestCommandContextResolver)commandContextResolver);
            testCommandContextResolver.Context.ShouldNotBeNull();
            testCommandContextResolver.Context.ShouldBeOfType<BamCommandContext>();
            BamCommandContext.Current.ServiceRegistry.ShouldBe(svcRegistry);

            // Make sure the static singleton returns expected instance
            commandContextResolver = BamCommandContext.Current.ServiceRegistry
                .Get<IBrokeredCommandContextResolver>();
            commandContextResolver.ShouldBeOfType<TestCommandContextResolver>($"Should have been of type {nameof(TestCommandContextResolver)} but was {commandContextResolver.GetType().Name}");
            testCommandContextResolver = ((TestCommandContextResolver)commandContextResolver);
            testCommandContextResolver.Context.ShouldNotBeNull();
            testCommandContextResolver.Context.ShouldBeOfType<BamCommandContext>();
        }
    }
}
