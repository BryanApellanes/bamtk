using System.Reflection;

namespace Bam
{
    /// <summary>
    /// Loads assemblies for reflection-driven generation and resolves their dependencies for as long as the
    /// probe lives. A dependency is looked for, by file name, in this order: beside the assembly that asks for
    /// it, in the directories of the other assemblies this probe loaded (in load order), and only then in the
    /// configured probe directories. A probe directory is therefore a fallback for a dependency that lives
    /// somewhere else (a sibling project's output, a package cache), and neither it nor another loaded
    /// assembly's directory shadows the copy sitting next to the assembly that needs it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hooks <see cref="AppDomain.AssemblyResolve"/>, which the runtime raises only after its own probing
    /// fails and which, unlike <c>AssemblyLoadContext.Resolving</c>, says which assembly is asking; unhooks it
    /// on <see cref="Dispose"/>. Assemblies are loaded into the default load context with
    /// <see cref="Assembly.LoadFrom(string)"/> because decorator generation reflects over runtime
    /// <see cref="Type"/>s; a metadata-only context would not do.
    /// </para>
    /// <para>
    /// Two limits follow from loading into one context. A dependency is matched on its simple name alone:
    /// version, culture and key token are not compared. And the process holds one assembly per simple name, so
    /// when two loaded assemblies each have their own build of a same-named dependency beside them, the build
    /// that is asked for first is the one both get for the rest of the process. Generate such services in
    /// separate runs.
    /// </para>
    /// </remarks>
    public sealed class AssemblyProbe : IDisposable
    {
        private static readonly string[] Extensions = new string[] { ".dll", ".exe" };

        private readonly List<string> _loadedDirectories = new List<string>();
        private readonly List<string> _probeDirectories = new List<string>();
        private bool _disposed;

        /// <summary>Initializes a probe and starts answering failed assembly resolutions.</summary>
        /// <param name="directories">The fallback directories to search for dependencies; null or empty for none to start with.</param>
        public AssemblyProbe(IEnumerable<string>? directories)
        {
            if (directories is not null)
            {
                foreach (string directory in directories)
                {
                    AddDirectory(directory);
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;
        }

        /// <summary>
        /// Gets the directories searched after the requesting assembly's own, as full paths, in search order:
        /// the directories of the assemblies this probe loaded, then the configured fallback directories.
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
        /// Loads the assembly at <paramref name="assemblyPath"/> and, once it has loaded, adds its directory to
        /// the directories searched ahead of the fallback ones.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly file.</param>
        /// <returns>The loaded assembly.</returns>
        /// <exception cref="FileNotFoundException">The file does not exist.</exception>
        /// <exception cref="BadImageFormatException">The file is not an assembly this runtime can load.</exception>
        public Assembly Load(string assemblyPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);

            FileInfo assemblyFile = new FileInfo(assemblyPath);
            if (!assemblyFile.Exists)
            {
                throw new FileNotFoundException($"Assembly not found: {assemblyPath}", assemblyPath);
            }

            Assembly assembly = Assembly.LoadFrom(assemblyFile.FullName);
            Add(_loadedDirectories, assemblyFile.DirectoryName);
            return assembly;
        }

        /// <summary>Stops answering failed assembly resolutions.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            AppDomain.CurrentDomain.AssemblyResolve -= Resolve;
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

        private Assembly? Resolve(object? sender, ResolveEventArgs args)
        {
            string? simpleName = new AssemblyName(args.Name).Name;
            if (string.IsNullOrEmpty(simpleName))
            {
                return null;
            }

            // Beside the assembly that asks first, so each loaded assembly gets the copy next to it; then the
            // other loaded assemblies' directories; a probe directory only answers for what is in none of them.
            List<string> searched = new List<string>();
            string? requestingLocation = args.RequestingAssembly?.Location;
            if (!string.IsNullOrEmpty(requestingLocation))
            {
                Add(searched, Path.GetDirectoryName(requestingLocation));
            }

            foreach (string directory in _loadedDirectories.Concat(_probeDirectories))
            {
                Add(searched, directory);
            }

            foreach (string directory in searched)
            {
                foreach (string extension in Extensions)
                {
                    string candidate = Path.Combine(directory, simpleName + extension);
                    if (File.Exists(candidate))
                    {
                        return Assembly.LoadFrom(candidate);
                    }
                }
            }

            return null;
        }
    }
}
