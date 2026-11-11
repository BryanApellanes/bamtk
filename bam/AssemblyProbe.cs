using System.Reflection;
using System.Runtime.Loader;

namespace Bam
{
    /// <summary>
    /// Loads assemblies for reflection-driven generation and resolves their dependencies for as long as the
    /// probe lives. A dependency is looked for, by file name, first in the directories of the assemblies this
    /// probe loaded and only then in the configured probe directories, so a probe directory is a fallback for
    /// a dependency that lives somewhere else (a sibling project's output, a package cache) and never shadows
    /// the copy sitting next to the assembly that needs it.
    /// </summary>
    /// <remarks>
    /// Hooks <see cref="AssemblyLoadContext.Resolving"/> on the default load context, which the runtime raises
    /// only after its own probing fails, and unhooks it on <see cref="Dispose"/>. That event is raised before
    /// <see cref="Assembly.LoadFrom(string)"/>'s own look beside the requesting assembly, which is why this
    /// type searches the loaded assemblies' directories itself, ahead of the configured ones. Assemblies are
    /// loaded into the default context (as <see cref="Assembly.LoadFrom(string)"/> does) because decorator
    /// generation reflects over runtime <see cref="Type"/>s; a metadata-only context would not do. A dependency
    /// is matched on its simple name alone: version, culture and key token are not compared.
    /// </remarks>
    public sealed class AssemblyProbe : IDisposable
    {
        private static readonly string[] Extensions = new string[] { ".dll", ".exe" };

        private readonly List<string> _loadedDirectories = new List<string>();
        private readonly List<string> _probeDirectories = new List<string>();
        private readonly AssemblyLoadContext _context;
        private bool _disposed;

        /// <summary>Initializes a probe over the default load context.</summary>
        /// <param name="directories">The fallback directories to search for dependencies; null or empty for none to start with.</param>
        public AssemblyProbe(IEnumerable<string>? directories)
            : this(AssemblyLoadContext.Default, directories)
        {
        }

        /// <summary>Initializes a probe over the specified load context.</summary>
        /// <param name="context">The load context whose failed resolutions this probe answers.</param>
        /// <param name="directories">The fallback directories to search for dependencies; null or empty for none to start with.</param>
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

        /// <summary>
        /// Gets the directories searched, as full paths, in search order: the directories of the assemblies
        /// this probe loaded, then the configured fallback directories.
        /// </summary>
        public IReadOnlyList<string> Directories => _loadedDirectories.Concat(_probeDirectories).ToList();

        /// <summary>
        /// Adds a fallback directory, searched after the directories of the loaded assemblies. A directory
        /// already present, or blank, is ignored.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public void AddDirectory(string? directory)
        {
            Add(_probeDirectories, directory);
        }

        /// <summary>
        /// Loads the assembly at <paramref name="assemblyPath"/> into the load context and adds its directory
        /// to the directories searched ahead of the fallback ones.
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

            Add(_loadedDirectories, assemblyFile.DirectoryName);
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

        private static void Add(List<string> directories, string? directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return;
            }

            string fullPath = Path.GetFullPath(directory);
            if (!directories.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
            {
                directories.Add(fullPath);
            }
        }

        private Assembly? Resolve(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName.Name))
            {
                return null;
            }

            // Beside the loaded assemblies first: a probe directory only answers for what is not there.
            foreach (string directory in _loadedDirectories.Concat(_probeDirectories))
            {
                foreach (string extension in Extensions)
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
