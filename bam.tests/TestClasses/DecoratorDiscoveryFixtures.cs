// Fixtures for decorator discovery (bam generate decorator, Discover:). They sit in their own namespace so a
// discovery scan of this assembly can be narrowed to exactly these types.
namespace Bam.Tests.TestClasses.Decorated
{
    /// <summary>An interface with exactly one public implementation: discovery pairs them.</summary>
    public interface IFarewellService
    {
        /// <summary>Says goodbye to the named person.</summary>
        string Farewell(string name);
    }

    /// <summary>The single implementation of <see cref="IFarewellService"/>.</summary>
    public class FarewellService : IFarewellService
    {
        /// <inheritdoc />
        public string Farewell(string name) => $"Goodbye, {name}";
    }

    /// <summary>An interface with two public implementations: discovery cannot pick one.</summary>
    public interface IAmbiguousService
    {
        /// <summary>Does something.</summary>
        int Choose(int value);
    }

    /// <summary>First implementation of <see cref="IAmbiguousService"/>.</summary>
    public class AmbiguousServiceA : IAmbiguousService
    {
        /// <inheritdoc />
        public int Choose(int value) => value;
    }

    /// <summary>Second implementation of <see cref="IAmbiguousService"/>.</summary>
    public class AmbiguousServiceB : IAmbiguousService
    {
        /// <inheritdoc />
        public int Choose(int value) => -value;
    }

    /// <summary>An interface with no public implementation: only an interface-only decorator is possible.</summary>
    public interface IUnimplementedService
    {
        /// <summary>Does nothing anyone has written yet.</summary>
        void Pending(string what);
    }

    /// <summary>The only implementation of <see cref="IUnimplementedService"/>, and it is not public.</summary>
    internal sealed class HiddenService : IUnimplementedService
    {
        /// <inheritdoc />
        public void Pending(string what)
        {
        }
    }
}

// What a project holds once it compiles the decorators a first run generated: classes that implement the
// service interfaces and derive from the decorator runtime's base. Discovery must not count them as
// implementations, or a second run over the rebuilt assembly would see two where the first saw one.
namespace Bam.Tests.TestClasses.Decorated.Decorators
{
    /// <summary>Stands in for the generated decorator of <see cref="FarewellService"/>.</summary>
    public class FarewellServiceDecorator : Bam.Generators.Decorators.Decorator<IFarewellService, FarewellService>, IFarewellService
    {
        /// <summary>Initializes a decorator around <paramref name="value"/>.</summary>
        public FarewellServiceDecorator(FarewellService value) : base(value)
        {
        }

        /// <inheritdoc />
        public string Farewell(string name) => Instance.Farewell(name);
    }

    /// <summary>Stands in for the generated interface-only decorator of <see cref="IUnimplementedService"/>.</summary>
    public class IUnimplementedServiceDecorator : Bam.Generators.Decorators.Decorator<IUnimplementedService, IUnimplementedService>, IUnimplementedService
    {
        /// <summary>Initializes a decorator around <paramref name="value"/>.</summary>
        public IUnimplementedServiceDecorator(IUnimplementedService value) : base(value)
        {
        }

        /// <inheritdoc />
        public void Pending(string what) => Instance.Pending(what);
    }
}

// Two interfaces of one simple name: their decorators would both be written to IClientDecorator.cs. They sit in
// namespaces of their own, outside the one the discovery tests scan.
namespace Bam.Tests.TestClasses.SameFile.Billing
{
    /// <summary>Shares its simple name with <see cref="Shipping.IClient"/>.</summary>
    public interface IClient
    {
        int Bill();
    }
}

namespace Bam.Tests.TestClasses.SameFile.Shipping
{
    /// <summary>Shares its simple name with <see cref="Billing.IClient"/>.</summary>
    public interface IClient
    {
        int Ship();
    }
}
