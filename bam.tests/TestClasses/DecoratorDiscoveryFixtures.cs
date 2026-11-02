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
