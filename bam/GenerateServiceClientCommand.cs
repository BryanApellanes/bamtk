using System.Reflection;
using Bam.Generators;

namespace Bam
{
    /// <summary>
    /// The console-independent action behind <c>bam generate service-client</c>: validates a
    /// <see cref="BamServiceClientGenerationConfig"/>, loads the target assembly, resolves the
    /// <c>[WebService]</c> type (and optional interface), and drives the shipped
    /// <see cref="BamServiceClientGenerator"/> to write a typed client to disk.
    /// </summary>
    /// <remarks>
    /// Deliberately free of any console/CLI dependency so it can be unit-tested directly (see
    /// <c>GenerateServiceClientCommandShould</c>). The menu command (<see cref="GenerateMenuContainer"/>) is a thin
    /// wrapper that delegates here and reports the outcome. Failures surface as thrown exceptions —
    /// <see cref="ArgumentException"/>/<see cref="FileNotFoundException"/> for bad input and
    /// <see cref="ServiceClientGenerationException"/> for generation errors (e.g. non-virtual methods in Subclass
    /// mode) — which the CLI turns into a non-zero exit.
    /// </remarks>
    public class GenerateServiceClientCommand
    {
        /// <summary>
        /// Generates the typed client described by <paramref name="config"/> and returns the path of the file written.
        /// </summary>
        /// <param name="config">The generation input. All of
        /// <see cref="BamServiceClientGenerationConfig.AssemblyPath"/>,
        /// <see cref="BamServiceClientGenerationConfig.ServiceTypeName"/> and
        /// <see cref="BamServiceClientGenerationConfig.OutputDirectory"/> are required.</param>
        /// <returns>The absolute-or-relative path of the generated <c>{ServiceName}Client.cs</c> file.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
        /// <exception cref="ArgumentException">A required field is missing, or a named type cannot be resolved.</exception>
        /// <exception cref="FileNotFoundException">The service assembly does not exist at <see cref="BamServiceClientGenerationConfig.AssemblyPath"/>.</exception>
        /// <exception cref="ServiceClientGenerationException">The service type is not a <c>[WebService]</c>, or Subclass mode was requested for a type with non-virtual remotable methods.</exception>
        public string Execute(BamServiceClientGenerationConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);
            RequireValue(config.AssemblyPath, nameof(config.AssemblyPath));
            RequireValue(config.ServiceTypeName, nameof(config.ServiceTypeName));
            RequireValue(config.OutputDirectory, nameof(config.OutputDirectory));

            FileInfo assemblyFile = new FileInfo(config.AssemblyPath);
            if (!assemblyFile.Exists)
            {
                throw new FileNotFoundException($"Service assembly not found: {config.AssemblyPath}", config.AssemblyPath);
            }

            Assembly assembly = Assembly.LoadFrom(assemblyFile.FullName);

            Type serviceType = assembly.GetType(config.ServiceTypeName)
                ?? throw new ArgumentException(
                    $"Service type '{config.ServiceTypeName}' was not found in assembly '{assemblyFile.FullName}'.");

            Type? interfaceType = null;
            if (config.Mode == GenerationMode.Interface && !string.IsNullOrWhiteSpace(config.InterfaceTypeName))
            {
                interfaceType = assembly.GetType(config.InterfaceTypeName)
                    ?? throw new ArgumentException(
                        $"Interface type '{config.InterfaceTypeName}' was not found in assembly '{assemblyFile.FullName}'.");
            }

            BamServiceClientGenerator generator = new BamServiceClientGenerator();
            generator.AddServiceType(serviceType, config.Mode, interfaceType).WriteSource(config.OutputDirectory);

            return Path.Combine(config.OutputDirectory, $"{serviceType.Name}Client.cs");
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
