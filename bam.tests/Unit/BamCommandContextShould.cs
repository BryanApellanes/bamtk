using Bam.Command;
using Bam.DependencyInjection;
using Bam.Test;
using Bam.Tests.TestClasses;

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

            When.A<ServiceRegistry>("uses configured context resolver generic type",
                svcRegistry,
                (reg) =>
                {
                    IBrokeredCommandContextResolver resolver = reg.Get<IBrokeredCommandContextResolver>();
                    bool isDefaultType = resolver is ProcessCommandContextResolver;

                    reg.For<IBrokeredCommandContextResolver>().Use<TestCommandContextResolver>();
                    resolver = reg.Get<IBrokeredCommandContextResolver>();
                    bool isTestType = resolver is TestCommandContextResolver;

                    bool registryIsSame = BamCommandContext.Current.ServiceRegistry == reg;

                    resolver = BamCommandContext.Current.ServiceRegistry.Get<IBrokeredCommandContextResolver>();
                    bool singletonIsTestType = resolver is TestCommandContextResolver;

                    return new object[] { isDefaultType, isTestType, registryIsSame, singletonIsTestType };
                })
            .TheTest
            .ShouldPass(because =>
            {
                object[] r = (object[])because.Result;
                because.ItsTrue("default resolver is ProcessCommandContextResolver", (bool)r[0]);
                because.ItsTrue("replaced resolver is TestCommandContextResolver", (bool)r[1]);
                because.ItsTrue("ServiceRegistry is same instance", (bool)r[2]);
                because.ItsTrue("singleton returns TestCommandContextResolver", (bool)r[3]);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void UseConfiguredContextResolverInstanciator()
        {
            ServiceRegistry svcRegistry = BamCommandContext.Current.ServiceRegistry;

            When.A<ServiceRegistry>("uses configured context resolver instanciator",
                svcRegistry,
                (reg) =>
                {
                    reg.For<IBrokeredCommandContextResolver>().Use<ProcessCommandContextResolver>();
                    IBrokeredCommandContextResolver resolver = reg.Get<IBrokeredCommandContextResolver>();
                    bool isDefaultType = resolver is ProcessCommandContextResolver;

                    reg.For<IBrokeredCommandContextResolver>().Use(() => new TestCommandContextResolver());
                    resolver = reg.Get<IBrokeredCommandContextResolver>();
                    bool isTestType = resolver is TestCommandContextResolver;

                    bool registryIsSame = BamCommandContext.Current.ServiceRegistry == reg;

                    resolver = BamCommandContext.Current.ServiceRegistry.Get<IBrokeredCommandContextResolver>();
                    bool singletonIsTestType = resolver is TestCommandContextResolver;

                    return new object[] { isDefaultType, isTestType, registryIsSame, singletonIsTestType };
                })
            .TheTest
            .ShouldPass(because =>
            {
                object[] r = (object[])because.Result;
                because.ItsTrue("default resolver is ProcessCommandContextResolver", (bool)r[0]);
                because.ItsTrue("replaced resolver is TestCommandContextResolver", (bool)r[1]);
                because.ItsTrue("ServiceRegistry is same instance", (bool)r[2]);
                because.ItsTrue("singleton returns TestCommandContextResolver", (bool)r[3]);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void UseConfiguredContextResolverParameterizedInstanciator()
        {
            ServiceRegistry svcRegistry = BamCommandContext.Current.ServiceRegistry;

            When.A<ServiceRegistry>("uses configured context resolver parameterized instanciator",
                svcRegistry,
                (reg) =>
                {
                    reg.For<IBrokeredCommandContextResolver>().Use<ProcessCommandContextResolver>();
                    IBrokeredCommandContextResolver resolver = reg.Get<IBrokeredCommandContextResolver>();
                    bool isDefaultType = resolver is ProcessCommandContextResolver;

                    reg.For<IBrokeredCommandContextResolver>().Use((svcReg) => new TestCommandContextResolver(svcReg.Get<IBamBrokeredCommandContext>()));
                    resolver = reg.Get<IBrokeredCommandContextResolver>();

                    bool isTestType = resolver is TestCommandContextResolver;
                    TestCommandContextResolver testResolver = (TestCommandContextResolver)resolver;
                    bool contextNotNull = testResolver.Context != null;
                    bool contextIsBamCommandContext = testResolver.Context is BamCommandContext;

                    bool registryIsSame = BamCommandContext.Current.ServiceRegistry == reg;

                    resolver = BamCommandContext.Current.ServiceRegistry.Get<IBrokeredCommandContextResolver>();
                    bool singletonIsTestType = resolver is TestCommandContextResolver;
                    testResolver = (TestCommandContextResolver)resolver;
                    bool singletonContextNotNull = testResolver.Context != null;
                    bool singletonContextIsBamCommandContext = testResolver.Context is BamCommandContext;

                    return new object[] { isDefaultType, isTestType, contextNotNull, contextIsBamCommandContext, registryIsSame, singletonIsTestType, singletonContextNotNull, singletonContextIsBamCommandContext };
                })
            .TheTest
            .ShouldPass(because =>
            {
                object[] r = (object[])because.Result;
                because.ItsTrue("default resolver is ProcessCommandContextResolver", (bool)r[0]);
                because.ItsTrue("replaced resolver is TestCommandContextResolver", (bool)r[1]);
                because.ItsTrue("Context is not null", (bool)r[2]);
                because.ItsTrue("Context is BamCommandContext", (bool)r[3]);
                because.ItsTrue("ServiceRegistry is same instance", (bool)r[4]);
                because.ItsTrue("singleton returns TestCommandContextResolver", (bool)r[5]);
                because.ItsTrue("singleton Context is not null", (bool)r[6]);
                because.ItsTrue("singleton Context is BamCommandContext", (bool)r[7]);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }
    }
}
