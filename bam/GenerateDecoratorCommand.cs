using System.Reflection;
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
    /// output directory, no services at all, a named assembly or template directory that does not exist —
    /// surface as thrown exceptions (<see cref="ArgumentException"/>, <see cref="FileNotFoundException"/>,
    /// <see cref="DirectoryNotFoundException"/>). Problems with one service — a type that cannot be resolved,
    /// a dependency that cannot be loaded, a pair the generator rejects, a decorator named like one already
    /// written in the run — are reported in the returned <see cref="DecoratorGenerationReport"/> and the run
    /// continues with the next service.
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
        /// <exception cref="FileNotFoundException">An assembly named by the config does not exist.</exception>
        /// <exception cref="DirectoryNotFoundException">The configured template directory does not exist.</exception>
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

            for (int i = 0; i < services.Count; i++)
            {
                RequireValue(services[i].Interface, $"{nameof(config.Services)}[{i}].{nameof(DecoratorServiceConfig.Interface)}");
                RequireValue(services[i].AssemblyPath ?? config.AssemblyPath, nameof(config.AssemblyPath));
            }

            if (config.Discover is not null)
            {
                RequireValue(config.Discover.AssemblyPath ?? config.AssemblyPath, $"{nameof(config.Discover)}.{nameof(DecoratorDiscoveryConfig.AssemblyPath)}");
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
            string interfaceAssemblyPath = service.AssemblyPath ?? config.AssemblyPath;
            string? implementationAssemblyPath = service.ImplementationAssemblyPath
                ?? (string.IsNullOrWhiteSpace(service.AssemblyPath) ? config.ImplementationAssemblyPath : null);
            string? implementationName = string.IsNullOrWhiteSpace(service.Implementation) ? null : service.Implementation;

            Assembly interfaceAssembly = probe.Load(interfaceAssemblyPath);
            Assembly implementationAssembly = string.IsNullOrWhiteSpace(implementationAssemblyPath)
                ? interfaceAssembly
                : probe.Load(implementationAssemblyPath);

            Type? interfaceType = Resolve(interfaceAssembly, service.Interface);
            if (interfaceType is null)
            {
                run.Skip(service.Interface, implementationName, $"interface type '{service.Interface}' was not found in assembly '{interfaceAssembly.Location}'.");
                return;
            }

            Type? implementationType = null;
            if (implementationName is not null)
            {
                implementationType = Resolve(implementationAssembly, implementationName);
                if (implementationType is null)
                {
                    run.Skip(service.Interface, implementationName, $"implementation type '{implementationName}' was not found in assembly '{implementationAssembly.Location}'.");
                    return;
                }
            }

            run.Generate(interfaceType, implementationType);
        }

        private static void Discover(GenerationRun run, AssemblyProbe probe, DecoratorDiscoveryConfig discovery)
        {
            Assembly assembly = probe.Load(discovery.AssemblyPath ?? run.Config.AssemblyPath);
            string assemblyName = assembly.GetName().Name ?? assembly.Location;

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(type => type is not null).Select(type => type!).ToArray();
                string detail = ex.LoaderExceptions.FirstOrDefault(e => e is not null)?.Message ?? ex.Message;
                run.Skip(assemblyName, null, $"some types in the assembly could not be loaded and were not scanned: {detail}");
            }

            IEnumerable<Type> interfaces = types
                .Where(type => type.IsInterface && type.IsVisible && !type.IsGenericTypeDefinition && InNamespace(type, discovery.Namespace))
                .OrderBy(type => type.FullName, StringComparer.Ordinal);

            foreach (Type interfaceType in interfaces)
            {
                List<Type> implementations;
                try
                {
                    implementations = types
                        .Where(type => type.IsClass && !type.IsAbstract && type.IsVisible && !type.IsGenericTypeDefinition && interfaceType.IsAssignableFrom(type))
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

        private static Type? Resolve(Assembly assembly, string typeName)
        {
            return assembly.GetType(typeName);
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
        /// and the decorator names already taken, so a later service that would write the same file is skipped
        /// instead of overwriting it.
        /// </summary>
        private sealed class GenerationRun
        {
            private readonly List<string> _written = new List<string>();
            private readonly List<SkippedDecorator> _skipped = new List<SkippedDecorator>();
            private readonly HashSet<string> _decoratorNames = new HashSet<string>(StringComparer.Ordinal);
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

            public void Generate(Type interfaceType, Type? implementationType)
            {
                string interfaceName = interfaceType.FullName ?? interfaceType.Name;
                string? implementationName = implementationType?.FullName ?? implementationType?.Name;
                string decoratorName = GeneratedDecorators.FullNameOf(implementationType ?? interfaceType);
                if (!_decoratorNames.Add(decoratorName))
                {
                    Skip(interfaceName, implementationName, $"would generate {decoratorName}, which this run already wrote; the two files would overwrite each other.");
                    return;
                }

                try
                {
                    DecoratorGenerator generator = new DecoratorGenerator(_codeWriter, new FsDecoratorTargetResolver());
                    if (!Config.RegistryExtensions)
                    {
                        generator.WithoutRegistryExtensions();
                    }

                    _written.AddRange(generator.AddServiceType(interfaceType, implementationType).WriteSource(Config.OutputDirectory));
                }
                catch (DecoratorGenerationException ex)
                {
                    _decoratorNames.Remove(decoratorName);
                    Skip(interfaceName, implementationName, ex.Message);
                }
                catch (Exception ex) when (IsLoadFailure(ex))
                {
                    _decoratorNames.Remove(decoratorName);
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
