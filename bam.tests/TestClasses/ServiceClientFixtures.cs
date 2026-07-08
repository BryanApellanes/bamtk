using Bam.Server;

namespace Bam.Tests.TestClasses
{
    /// <summary>
    /// A <c>[WebService]</c> fixture with virtual sync and async remotable methods — valid input for
    /// Subclass-mode service-client generation.
    /// </summary>
    [WebService]
    public class EchoWebService
    {
        /// <summary>Echoes the supplied message back to the caller.</summary>
        public virtual string Echo(string message) => message;

        /// <summary>Returns the sum of two integers asynchronously.</summary>
        public virtual Task<int> AddAsync(int a, int b) => Task.FromResult(a + b);
    }

    /// <summary>
    /// A <c>[WebService]</c> fixture whose remotable method is NOT virtual — Subclass-mode generation must reject it.
    /// </summary>
    [WebService]
    public class NonVirtualWebService
    {
        /// <summary>A non-virtual method that prevents Subclass-mode generation.</summary>
        public string Ping() => "pong";
    }
}
