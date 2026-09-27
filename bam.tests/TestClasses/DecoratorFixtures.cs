namespace Bam.Tests.TestClasses
{
    /// <summary>A service interface — valid input for decorator generation.</summary>
    public interface IGreetingService
    {
        /// <summary>Greets the named person.</summary>
        string Greet(string name);

        /// <summary>Greets the named person asynchronously.</summary>
        Task<string> GreetAsync(string name);
    }

    /// <summary>The implementation of <see cref="IGreetingService"/> the generated decorator wraps.</summary>
    public class GreetingService : IGreetingService
    {
        /// <inheritdoc />
        public string Greet(string name) => $"Hello, {name}";

        /// <inheritdoc />
        public Task<string> GreetAsync(string name) => Task.FromResult(Greet(name));
    }

    /// <summary>Does NOT implement <see cref="IGreetingService"/> — decorator generation must reject it.</summary>
    public class NotAGreetingService
    {
        /// <summary>Looks like the service method, but the type does not implement the interface.</summary>
        public string Greet(string name) => name;
    }
}
