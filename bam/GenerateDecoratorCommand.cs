using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Bam.Generators;
using Bam.Generators.Decorators;

namespace Bam
{
    /// <summary>
    /// The console-independent action behind <c>bam generate decorator</c>: validates a
    /// <see cref="BamDecoratorGenerationConfig"/>, loads the target assemblies (resolving their dependencies
    /// from the configured probe directories), resolves or discovers the services, and drives the shipped
    /// <see cref="DecoratorGenerator"/> to write one typed decorator per service to disk.
    /// </summary>
    /// <remarks>
    /// Deliberately free of any console/CLI dependency so it can be unit-tested directly (see
    /// <c>GenerateDecoratorCommandShould</c>). The menu command (<see cref="GenerateMenuContainer"/>) is a thin
    /// wrapper that delegates here and reports the outcome. Problems with the config as a whole — a missing
    /// output directory, no services at all, a named assembly, probe directory or template directory that does
    /// not exist — surface as thrown exceptions (<see cref="ArgumentException"/>,
    /// <see cref="FileNotFoundException"/>, <see cref="DirectoryNotFoundException"/>) before anything is
    /// written. Problems with one service — an assembly that is not a loadable assembly, a type that cannot be
    /// resolved, a dependency that cannot be loaded, a pair the generator rejects, a decorator named like one
    /// already written in the run — are reported in the returned <see cref="DecoratorGenerationReport"/> and
    /// the run continues with the next service.
    /// </remarks>
    public class GenerateDecoratorCommand
    {
        /// <summary>
        /// Generates a decorator for every service described by <paramref name="config"/> and reports what was
        /// written and what was skipped.
        /// </summary>
        /// <param name="config">The generation input. <see cref="BamDecoratorGenerationConfig.OutputDirectory"/>
        /// and at least one service source (<see cref="BamDecoratorGenerationConfig.InterfaceTypeName"/>,
        /// <see cref="BamDecoratorGenerationConfig.Services"/> or <see cref="BamDecoratorGenerationConfig.Discover"/>)
        /// are required, and every service needs an assembly path.</param>
        /// <returns>The files written and the services skipped, with reasons.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">A required field is missing, or no service is configured.</exception>
        /// <exception cref="FileNotFoundException">An assembly named anywhere in the config does not exist. Thrown before anything is written.</exception>
        /// <exception cref="DirectoryNotFoundException">The configured template directory, or a probe directory, does not exist.</exception>
        public DecoratorGenerationReport Execute(BamDecoratorGenerationConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            RequireValue(config.OutputDirectory, nameof(config.OutputDirectory));

            IReadOnlyList<DecoratorServiceConfig> services = config.GetServices();
            if (services.Count == 0 && config.Discover is null)
            {
                throw new ArgumentException(
                    $"No service to decorate: set {nameof(config.InterfaceTypeName)}, add a {nameof(config.Services)} entry or set {nameof(config.Discover)}.",
                    nameof(config.Services));
            }

            // Everything the config names is checked before the first file is written, so a mistyped path in a
            // later entry never leaves a half-finished run behind with no report of what it wrote.
            for (int i = 0; i < services.Count; i++)
            {
                if (services[i] is null)
                {
                    throw new ArgumentException($"{nameof(config.Services)}[{i}] is empty.", nameof(config.Services));
                }

                RequireValue(services[i].Interface, $"{nameof(config.Services)}[{i}].{nameof(DecoratorServiceConfig.Interface)}");
                string? interfaceAssemblyPath = InterfaceAssemblyPathOf(config, services[i]);
                RequireValue(interfaceAssemblyPath, nameof(config.AssemblyPath));
                RequireAssemblyFile(interfaceAssemblyPath!);
                string? implementationAssemblyPath = ImplementationAssemblyPathOf(config, services[i]);
                if (implementationAssemblyPath is not null)
                {
                    RequireAssemblyFile(implementationAssemblyPath);
                }
            }

            if (config.Discover is not null)
            {
                string? discoveryAssemblyPath = FirstSet(config.Discover.AssemblyPath, config.AssemblyPath);
                RequireValue(discoveryAssemblyPath, $"{nameof(config.Discover)}.{nameof(DecoratorDiscoveryConfig.AssemblyPath)}");
                RequireAssemblyFile(discoveryAssemblyPath!);
            }

            if (config.ProbeDirectories is not null)
            {
                foreach (string probeDirectory in config.ProbeDirectories)
                {
                    if (!string.IsNullOrWhiteSpace(probeDirectory) && !Directory.Exists(probeDirectory))
                    {
                        throw new DirectoryNotFoundException($"{nameof(config.ProbeDirectories)} entry not found: {probeDirectory}");
                    }
                }
            }

            IDecoratorCodeWriter codeWriter = CreateCodeWriter(config.TemplateDirectory);
            using AssemblyProbe probe = new AssemblyProbe(config.ProbeDirectories);
            GenerationRun run = new GenerationRun(config, codeWriter);

            foreach (DecoratorServiceConfig service in services)
            {
                GenerateListed(run, probe, service);
            }

            if (config.Discover is not null)
            {
                Discover(run, probe, config.Discover);
            }

            return run.ToReport();
        }

        private static void GenerateListed(GenerationRun run, AssemblyProbe probe, DecoratorServiceConfig service)
        {
            BamDecoratorGenerationConfig config = run.Config;
            string interfaceAssemblyPath = InterfaceAssemblyPathOf(config, service)!;
            string? implementationAssemblyPath = ImplementationAssemblyPathOf(config, service);
            string? implementationName = string.IsNullOrWhiteSpace(service.Implementation) ? null : service.Implementation;

            Assembly interfaceAssembly;
            Assembly implementationAssembly;
            try
            {
                interfaceAssembly = probe.Load(interfaceAssemblyPath);
                implementationAssembly = implementationAssemblyPath is null
                    ? interfaceAssembly
                    : probe.Load(implementationAssemblyPath);
            }
            catch (Exception ex) when (IsLoadFailure(ex))
            {
                // The files exist (checked up front); one of them is not an assembly this runtime can load.
                run.Skip(service.Interface, implementationName, $"an assembly could not be loaded: {ex.Message}");
                return;
            }

            Type? interfaceType = Resolve(interfaceAssembly, service.Interface, out string? interfaceLoadFailure);
            if (interfaceType is null)
            {
                run.Skip(service.Interface, implementationName, interfaceLoadFailure is null
                    ? $"interface type '{service.Interface}' was not found in assembly '{interfaceAssembly.Location}'."
                    : $"interface type '{service.Interface}' is in assembly '{interfaceAssembly.Location}', but a dependency could not be loaded: {interfaceLoadFailure}");
                return;
            }

            // From here the interface belongs to this entry, generated or not: discovery passes over it.
            run.MarkListed(interfaceType);

            Type? implementationType = null;
            if (implementationName is not null)
            {
                implementationType = Resolve(implementationAssembly, implementationName, out string? implementationLoadFailure);
                if (implementationType is null)
                {
                    run.Skip(service.Interface, implementationName, implementationLoadFailure is null
                        ? $"implementation type '{implementationName}' was not found in assembly '{implementationAssembly.Location}'."
                        : $"implementation type '{implementationName}' is in assembly '{implementationAssembly.Location}', but a dependency could not be loaded: {implementationLoadFailure}");
                    return;
                }
            }

            run.Generate(interfaceType, implementationType);
        }

        private static void Discover(GenerationRun run, AssemblyProbe probe, DecoratorDiscoveryConfig discovery)
        {
            string assemblyPath = FirstSet(discovery.AssemblyPath, run.Config.AssemblyPath)!;
            Assembly assembly;
            try
            {
                assembly = probe.Load(assemblyPath);
            }
            catch (Exception ex) when (IsLoadFailure(ex))
            {
                run.Skip(assemblyPath, null, $"the assembly could not be loaded and was not scanned: {ex.Message}");
                return;
            }

            string assemblyName = assembly.GetName().Name ?? assembly.Location;

            Type[] types;
            bool partialScan = false;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                partialScan = true;
                types = ex.Types.Where(type => type is not null).Select(type => type!).ToArray();
                string detail = ex.LoaderExceptions.FirstOrDefault(e => e is not null)?.Message ?? ex.Message;
                run.Skip(assemblyName, null, $"some types in the assembly could not be loaded and were not scanned: {detail}");
            }

            List<Type> interfaces = types
                .Where(type => type.IsInterface && type.IsVisible && !type.IsGenericTypeDefinition && InNamespace(type, discovery.Namespace))
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToList();

            if (interfaces.Count == 0)
            {
                run.Skip(assemblyName, null, string.IsNullOrWhiteSpace(discovery.Namespace)
                    ? "no public interface was found in the assembly."
                    : $"no public interface was found in namespace '{discovery.Namespace}' or below it.");
                return;
            }

            foreach (Type interfaceType in interfaces)
            {
                // A Services entry (or the single pair) that named this interface has the say over it.
                if (run.IsListed(interfaceType))
                {
                    continue;
                }

                List<Type> implementations;
                try
                {
                    // A generated decorator implements the interface it decorates and is usually compiled into
                    // the assembly being scanned; it is never a candidate, or a second run would see two
                    // implementations where the first saw one.
                    implementations = types
                        .Where(type => type.IsClass && !type.IsAbstract && type.IsVisible && !type.IsGenericTypeDefinition && interfaceType.IsAssignableFrom(type) && !IsDecorator(type))
                        .OrderBy(type => type.FullName, StringComparer.Ordinal)
                        .ToList();
                }
                catch (Exception ex) when (IsLoadFailure(ex))
                {
                    run.Skip(interfaceType.FullName ?? interfaceType.Name, null, $"a dependency could not be loaded: {ex.Message}");
                    continue;
                }

                if (implementations.Count == 1)
                {
                    run.Generate(interfaceType, implementations[0]);
                }
                else if (discovery.InterfaceOnly && partialScan)
                {
                    // The implementation may be among the types that did not load. Falling back to the interface
                    // alone here would change the decorator's type for a reason the config never asked for.
                    run.Skip(interfaceType.FullName ?? interfaceType.Name, null,
                        $"{implementations.Count} public implementations were found, but some types in '{assemblyName}' could not be loaded, so its implementation may be among them; fix the load failure, or list the service under Services.");
                }
                else if (discovery.InterfaceOnly)
                {
                    run.Generate(interfaceType, null);
                }
                else if (implementations.Count == 0)
                {
                    run.Skip(interfaceType.FullName ?? interfaceType.Name, null,
                        $"no public implementation in '{assemblyName}'; set {nameof(DecoratorDiscoveryConfig.InterfaceOnly)} to decorate the interface alone.");
                }
                else
                {
                    string names = string.Join(", ", implementations.Select(type => type.FullName));
                    run.Skip(interfaceType.FullName ?? interfaceType.Name, null,
                        $"{implementations.Count} public implementations in '{assemblyName}' ({names}); list the pair you want under Services, or set {nameof(DecoratorDiscoveryConfig.InterfaceOnly)} to decorate the interface alone.");
                }
            }
        }

        private static bool InNamespace(Type type, string? @namespace)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
            {
                return true;
            }

            string? actual = type.Namespace;
            return actual is not null
                && (actual.Equals(@namespace, StringComparison.Ordinal) || actual.StartsWith(@namespace + ".", StringComparison.Ordinal));
        }

        private static IDecoratorCodeWriter CreateCodeWriter(string? templateDirectory)
        {
            if (string.IsNullOrWhiteSpace(templateDirectory))
            {
                return new HandlebarsDecoratorCodeWriter();
            }

            if (!Directory.Exists(templateDirectory))
            {
                throw new DirectoryNotFoundException($"{nameof(BamDecoratorGenerationConfig.TemplateDirectory)} not found: {templateDirectory}");
            }

            return new HandlebarsDecoratorCodeWriter(
                new HandlebarsDirectory(templateDirectory),
                new HandlebarsEmbeddedResources(typeof(HandlebarsDecoratorCodeWriter).Assembly));
        }

        // Null with a null loadFailure means the assembly holds no type of that name; null with a loadFailure
        // means the type is there but could not be loaded (a dependency is missing, or is a build the type does
        // not fit). Assembly.GetType(name) alone returns null for both, and the TypeLoadException it throws
        // when asked to names the requested type in both, so absence is read from the assembly's own metadata.
        private static Type? Resolve(Assembly assembly, string typeName, out string? loadFailure)
        {
            loadFailure = null;
            try
            {
                return assembly.GetType(typeName, true);
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (Exception ex) when (IsLoadFailure(ex))
            {
                if (ex is TypeLoadException && !DefinesType(assembly, typeName))
                {
                    return null;
                }

                loadFailure = ex.Message;
                return null;
            }
        }

        // Whether the assembly file's type-definition table holds a type of that full name (nested types
        // spelled Outer+Inner, as Assembly.GetType takes them). Reads metadata only; loads nothing.
        private static bool DefinesType(Assembly assembly, string typeName)
        {
            try
            {
                using FileStream stream = new FileStream(assembly.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using PEReader peReader = new PEReader(stream);
                MetadataReader reader = peReader.GetMetadataReader();
                foreach (TypeDefinitionHandle handle in reader.TypeDefinitions)
                {
                    if (FullNameOf(reader, reader.GetTypeDefinition(handle)) == typeName)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex) when (ex is IOException || ex is BadImageFormatException || ex is InvalidOperationException || ex is ArgumentException || ex is UnauthorizedAccessException)
            {
                // The file cannot be read back; "not found" is then the runtime's word for it.
                return false;
            }
        }

        private static string FullNameOf(MetadataReader reader, TypeDefinition definition)
        {
            string name = reader.GetString(definition.Name);
            TypeDefinitionHandle declaring = definition.GetDeclaringType();
            if (!declaring.IsNil)
            {
                return FullNameOf(reader, reader.GetTypeDefinition(declaring)) + "+" + name;
            }

            string @namespace = reader.GetString(definition.Namespace);
            return @namespace.Length == 0 ? name : @namespace + "." + name;
        }

        // True for a type built on the decorator runtime's Decorator<T> (every generated decorator is). Compared
        // by name so a target compiled against another build of the runtime is recognised too.
        private static bool IsDecorator(Type type)
        {
            string? decoratorBase = typeof(Decorator<>).FullName;
            for (Type? current = type.BaseType; current is not null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition().FullName == decoratorBase)
                {
                    return true;
                }
            }

            return false;
        }

        // Blank counts as unset everywhere a path falls back to another.
        private static string? FirstSet(string? first, string? second)
        {
            if (!string.IsNullOrWhiteSpace(first))
            {
                return first;
            }

            return string.IsNullOrWhiteSpace(second) ? null : second;
        }

        private static string? InterfaceAssemblyPathOf(BamDecoratorGenerationConfig config, DecoratorServiceConfig service)
        {
            return FirstSet(service.AssemblyPath, config.AssemblyPath);
        }

        // An entry with an assembly of its own does not inherit the top-level implementation assembly: that one
        // goes with the top-level AssemblyPath.
        private static string? ImplementationAssemblyPathOf(BamDecoratorGenerationConfig config, DecoratorServiceConfig service)
        {
            return FirstSet(service.ImplementationAssemblyPath, string.IsNullOrWhiteSpace(service.AssemblyPath) ? config.ImplementationAssemblyPath : null);
        }

        private static void RequireAssemblyFile(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException($"Assembly not found: {assemblyPath}", assemblyPath);
            }
        }

        private static bool IsLoadFailure(Exception ex)
        {
            return ex is FileNotFoundException
                || ex is FileLoadException
                || ex is BadImageFormatException
                || ex is TypeLoadException
                || ex is ReflectionTypeLoadException;
        }

        private static void RequireValue(string? value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{name} is required.", name);
            }
        }

        /// <summary>
        /// The state of one <see cref="Execute"/> call: the files written so far, the services skipped so far,
        /// and the output file names already taken, so a later service that would write the same file is skipped
        /// instead of overwriting it. Files are named after the decorator type alone and land in one directory,
        /// so two services named alike in different namespaces take the same file.
        /// </summary>
        private sealed class GenerationRun
        {
            private readonly List<string> _written = new List<string>();
            private readonly List<SkippedDecorator> _skipped = new List<SkippedDecorator>();
            private readonly Dictionary<string, string> _decoratorsByFileName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            private readonly HashSet<Type> _listed = new HashSet<Type>();
            private readonly IDecoratorCodeWriter _codeWriter;

            public GenerationRun(BamDecoratorGenerationConfig config, IDecoratorCodeWriter codeWriter)
            {
                Config = config;
                _codeWriter = codeWriter;
            }

            public BamDecoratorGenerationConfig Config { get; }

            public void Skip(string interfaceName, string? implementationName, string reason)
            {
                _skipped.Add(new SkippedDecorator(interfaceName, implementationName, reason));
            }

            public void MarkListed(Type interfaceType)
            {
                _listed.Add(interfaceType);
            }

            public bool IsListed(Type interfaceType)
            {
                return _listed.Contains(interfaceType);
            }

            public void Generate(Type interfaceType, Type? implementationType)
            {
                string interfaceName = interfaceType.FullName ?? interfaceType.Name;
                string? implementationName = implementationType?.FullName ?? implementationType?.Name;
                string decoratorName = GeneratedDecorators.FullNameOf(implementationType ?? interfaceType);
                string fileName = GeneratedDecorators.TypeNameOf(implementationType ?? interfaceType) + ".cs";
                if (_decoratorsByFileName.TryGetValue(fileName, out string? written))
                {
                    Skip(interfaceName, implementationName, written == decoratorName
                        ? $"would generate {decoratorName}, which this run already wrote; the two files would overwrite each other."
                        : $"would write {decoratorName} to {fileName}, the file this run already wrote {written} to; the second would overwrite the first. Generate it in a run with its own {nameof(BamDecoratorGenerationConfig.OutputDirectory)}.");
                    return;
                }

                _decoratorsByFileName.Add(fileName, decoratorName);

                try
                {
                    DecoratorGenerator generator = new DecoratorGenerator(_codeWriter, new FsDecoratorTargetResolver());
                    if (!Config.RegistryExtensions)
                    {
                        generator.WithoutRegistryExtensions();
                    }

                    // A null implementation is only ever a request for the interface alone: a named
                    // implementation that was not found is skipped before it gets here.
                    if (implementationType is null)
                    {
                        generator.AddInterface(interfaceType);
                    }
                    else
                    {
                        generator.AddServiceType(interfaceType, implementationType);
                    }

                    _written.AddRange(generator.WriteSource(Config.OutputDirectory));
                }
                catch (DecoratorGenerationException ex)
                {
                    _decoratorsByFileName.Remove(fileName);
                    Skip(interfaceName, implementationName, ex.Message);
                }
                catch (Exception ex) when (IsLoadFailure(ex))
                {
                    _decoratorsByFileName.Remove(fileName);
                    Skip(interfaceName, implementationName, $"a dependency could not be loaded: {ex.Message}");
                }
            }

            public DecoratorGenerationReport ToReport()
            {
                return new DecoratorGenerationReport(_written.AsReadOnly(), _skipped.AsReadOnly());
            }
        }
    }
}
