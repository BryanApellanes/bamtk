using System.IO.Pipes;

namespace Bam.Tests.TestClasses
{
    /// <summary>
    /// Detects an attempt to open a file, not only a load: named-pipe servers listen at
    /// <c>\\.\pipe\&lt;name&gt;</c> and at that name with each extension the runtime or a probe might add, so
    /// any open of one shows up as a connection (bamtk#133; the outbound-SMB case from bamtk#111). Windows
    /// only: <see cref="IsSupported"/> is false elsewhere, where <c>\\.\pipe\</c> is not a device path.
    /// </summary>
    public sealed class PipeOracle : IDisposable
    {
        private static readonly string[] Suffixes = new string[] { string.Empty, ".dll", ".exe", ".ni.dll", ".resources.dll", ".netmodule" };

        private readonly List<NamedPipeServerStream> _servers = new List<NamedPipeServerStream>();
        private readonly List<Task> _waits = new List<Task>();
        private readonly CancellationTokenSource _stop = new CancellationTokenSource();

        /// <summary>Starts a listener for <paramref name="name"/> and each suffixed form of it.</summary>
        /// <param name="name">The pipe name, unique to the test.</param>
        public PipeOracle(string name)
        {
            Name = name;
            if (!IsSupported)
            {
                return;
            }

            foreach (string suffix in Suffixes)
            {
                NamedPipeServerStream server = new NamedPipeServerStream(name + suffix, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                _servers.Add(server);
                _waits.Add(server.WaitForConnectionAsync(_stop.Token));
            }
        }

        /// <summary>Gets a value indicating whether named pipes can be watched on this platform.</summary>
        public static bool IsSupported => OperatingSystem.IsWindows();

        /// <summary>Gets the pipe name the oracle watches.</summary>
        public string Name { get; }

        /// <summary>Gets the path a reference name would have to be to open the watched pipe.</summary>
        public string Path => @"\\.\pipe\" + Name;

        /// <summary>
        /// Gets the number of watched pipes that saw a connection, after waiting up to <paramref name="settle"/>
        /// for the first one. Returns as soon as any pipe connects, so a control that expects a connection does
        /// not sleep out the whole wait; an oracle that expects none waits the full <paramref name="settle"/>.
        /// </summary>
        /// <param name="settle">How long to wait for a first connection.</param>
        /// <returns>The count of pipes opened.</returns>
        public int Connections(TimeSpan settle)
        {
            if (!IsSupported)
            {
                return 0;
            }

            Task.WhenAny(Task.WhenAny(_waits), Task.Delay(settle)).Wait();
            return _waits.Count(wait => wait.Status == TaskStatus.RanToCompletion);
        }

        /// <summary>Stops listening and closes every pipe.</summary>
        public void Dispose()
        {
            _stop.Cancel();
            foreach (NamedPipeServerStream server in _servers)
            {
                server.Dispose();
            }

            _stop.Dispose();
        }
    }
}
