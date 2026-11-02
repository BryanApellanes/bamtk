using System.Reflection;
using System.Runtime.Loader;

namespace Bam
{
    /// <summary>
    /// Loads assemblies for reflection-driven generation and resolves their dependencies from a set of probe
    /// directories for as long as the probe lives. The runtime resolves a loaded assembly's dependencies from
    /// the application's own directory and from the directory the assembly was loaded from; a dependency that
    /// lives anywhere else (a sibling project's output, a package cache) is found here, by file name, in the
    /// configured directories and in the directories of the assemblies this probe loaded.
    /// </summary>
    /// <remarks>
    /// Hooks <see cref="AssemblyLoadContext.Resolving"/> on the default load context, which the runtime raises
    /// only after its own probing fails, and unhooks it on <see cref="Dispose"/>. Assemblies are loaded into the
    /// default context (as <see cref="Assembly.LoadFrom(string)"/> does) because decorator generation reflects
    /// over runtime <see cref="Type"/>s; a metadata-only context would not do.
    /// </remarks>
    public sealed class AssemblyProbe : IDisposable
    {
        private readonly List<string> _directories = new List<string>();
        private readonly AssemblyLoadContext _context;
        private bool _disposed;

        /// <summary>Initializes a probe over the default load context.</summary>
        /// <param name="directories">The directories to search for dependencies; null or empty for none to start with.</param>
        public AssemblyProbe(IEnumerable<string>? directories)
            : this(AssemblyLoadContext.Default, directories)
        {
        }

        /// <summary>Initializes a probe over the specified load context.</summary>
        /// <param name="context">The load context whose failed resolutions this probe answers.</param>
        /// <param name="directories">The directories to search for dependencies; null or empty for none to start with.</param>
        public AssemblyProbe(AssemblyLoadContext context, IEnumerable<string>? directories)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
            if (directories is not null)
            {
                foreach (string directory in directories)
                {
                    AddDirectory(directory);
                }
            }

            _context.Resolving += Resolve;
        }

        /// <summary>Gets the directories searched, as full paths, in search order.</summary>
        public IReadOnlyList<string> Directories => _directories;

        /// <summary>Adds a directory to search. A directory already present, or blank, is ignored.</summary>
        /// <param name="directory">The directory.</param>
        public void AddDirectory(string? directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return;
            }

            string fullPath = Path.GetFullPath(directory);
            if (!_directories.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
            {
                _directories.Add(fullPath);
            }
        }

        /// <summary>
        /// Loads the assembly at <paramref name="assemblyPath"/> into the load context and adds its directory
        /// to the directories searched.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly file.</param>
        /// <returns>The loaded assembly.</returns>
        /// <exception cref="FileNotFoundException">The file does not exist.</exception>
        public Assembly Load(string assemblyPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);

            FileInfo assemblyFile = new FileInfo(assemblyPath);
            if (!assemblyFile.Exists)
            {
                throw new FileNotFoundException($"Assembly not found: {assemblyPath}", assemblyPath);
            }

            AddDirectory(assemblyFile.DirectoryName);
            return _context == AssemblyLoadContext.Default
                ? Assembly.LoadFrom(assemblyFile.FullName)
                : _context.LoadFromAssemblyPath(assemblyFile.FullName);
        }

        /// <summary>Stops answering the load context's resolution requests.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _context.Resolving -= Resolve;
        }

        private Assembly? Resolve(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName.Name))
            {
                return null;
            }

            foreach (string directory in _directories)
            {
                foreach (string extension in new[] { ".dll", ".exe" })
                {
                    string candidate = Path.Combine(directory, assemblyName.Name + extension);
                    if (File.Exists(candidate))
                    {
                        return context.LoadFromAssemblyPath(candidate);
                    }
                }
            }

            return null;
        }
    }
}
