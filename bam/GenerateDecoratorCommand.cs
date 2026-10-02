using System.Reflection;
using Bam.Generators.Decorators;

namespace Bam
{
    /// <summary>
    /// The console-independent action behind <c>bam generate decorator</c>: validates a
    /// <see cref="BamDecoratorGenerationConfig"/>, loads the target assembly, resolves the service interface
    /// and its implementation, and drives the shipped <see cref="DecoratorGenerator"/> to write a typed
    /// decorator to disk.
    /// </summary>
    /// <remarks>
    /// Deliberately free of any console/CLI dependency so it can be unit-tested directly (see
    /// <c>GenerateDecoratorCommandShould</c>). The menu command (<see cref="GenerateMenuContainer"/>) is a thin
    /// wrapper that delegates here and reports the outcome. Failures surface as thrown exceptions —
    /// <see cref="ArgumentException"/>/<see cref="FileNotFoundException"/> for bad input and
    /// <see cref="DecoratorGenerationException"/> for generation errors (e.g. the implementation does not
    /// implement the interface) — which the CLI turns into a non-zero exit.
    /// </remarks>
    public class GenerateDecoratorCommand
    {
        /// <summary>
        /// Generates the typed decorator described by <paramref name="config"/> and returns the path of the file written.
        /// </summary>
        /// <param name="config">The generation input. All of
        /// <see cref="BamDecoratorGenerationConfig.AssemblyPath"/>,
        /// <see cref="BamDecoratorGenerationConfig.InterfaceTypeName"/>,
        /// <see cref="BamDecoratorGenerationConfig.ImplementationTypeName"/> and
        /// <see cref="BamDecoratorGenerationConfig.OutputDirectory"/> are required.</param>
        /// <returns>The absolute-or-relative path of the generated <c>{ImplementationName}Decorator.cs</c> file.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">A required field is missing, or a named type cannot be resolved.</exception>
        /// <exception cref="FileNotFoundException">An assembly named by the config does not exist.</exception>
        /// <exception cref="DecoratorGenerationException">The interface and implementation cannot be decorated.</exception>
        public string Execute(BamDecoratorGenerationConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            RequireValue(config.AssemblyPath, nameof(config.AssemblyPath));
            RequireValue(config.InterfaceTypeName, nameof(config.InterfaceTypeName));
            RequireValue(config.ImplementationTypeName, nameof(config.ImplementationTypeName));
            RequireValue(config.OutputDirectory, nameof(config.OutputDirectory));

            Assembly interfaceAssembly = Load(config.AssemblyPath);
            Assembly implementationAssembly = string.IsNullOrWhiteSpace(config.ImplementationAssemblyPath)
                ? interfaceAssembly
                : Load(config.ImplementationAssemblyPath);

            Type interfaceType = Resolve(interfaceAssembly, config.InterfaceTypeName, "Interface");
            Type implementationType = Resolve(implementationAssembly, config.ImplementationTypeName, "Implementation");

            DecoratorGenerator generator = new DecoratorGenerator();
            IReadOnlyList<string> written = generator
                .AddServiceType(interfaceType, implementationType)
                .WriteSource(config.OutputDirectory);

            return written[0];
        }

        private static Assembly Load(string assemblyPath)
        {
            FileInfo assemblyFile = new FileInfo(assemblyPath);
            if (!assemblyFile.Exists)
            {
                throw new FileNotFoundException($"Assembly not found: {assemblyPath}", assemblyPath);
            }

            return Assembly.LoadFrom(assemblyFile.FullName);
        }

        private static Type Resolve(Assembly assembly, string typeName, string role)
        {
            return assembly.GetType(typeName)
                ?? throw new ArgumentException(
                    $"{role} type '{typeName}' was not found in assembly '{assembly.Location}'.");
        }

        private static void RequireValue(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{name} is required.", name);
            }
        }
    }
}
