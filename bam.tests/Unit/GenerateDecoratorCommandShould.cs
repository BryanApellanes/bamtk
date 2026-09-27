using Bam.DependencyInjection;
using Bam.Generators.Decorators;
using Bam.Test;
using Bam.Tests.TestClasses;

namespace Bam.Tests.Unit
{
    [UnitTestMenu("GenerateDecoratorCommand should")]
    public class GenerateDecoratorCommandShould : UnitTestMenuContainer
    {
        public GenerateDecoratorCommandShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        private static string TestOutputDir(string name) =>
            Path.Combine(Path.GetTempPath(), "bam-decorator-tests", name);

        private static BamDecoratorGenerationConfig ValidConfig(string outputDirectory)
        {
            return new BamDecoratorGenerationConfig
            {
                AssemblyPath = typeof(IGreetingService).Assembly.Location,
                InterfaceTypeName = typeof(IGreetingService).FullName!,
                ImplementationTypeName = typeof(GreetingService).FullName!,
                OutputDirectory = outputDirectory
            };
        }

        [UnitTest(RunSynchronously = true)]
        public void ReadConfigFromYaml()
        {
            string dir = TestOutputDir("roundtrip");
            Directory.CreateDirectory(dir);
            string yamlPath = Path.Combine(dir, "config.yaml");
            File.WriteAllText(yamlPath,
                "AssemblyPath: ./MyService.dll\n" +
                "InterfaceTypeName: My.Ns.IMyService\n" +
                "ImplementationTypeName: My.Ns.MyService\n" +
                "ImplementationAssemblyPath: ./MyService.Impl.dll\n" +
                "OutputDirectory: ./out\n");

            After.Setup(reg =>
            {
                reg.For<FileInfo>().Use(new FileInfo(yamlPath));
            })
            .When<FileInfo>("reads a decorator config from YAML", file =>
            {
                return BamDecoratorGenerationConfig.ReadFrom(file.FullName);
            })
            .TheTest
            .ShouldPass<BamDecoratorGenerationConfig>((because, config) =>
            {
                because.ItsTrue("AssemblyPath is parsed", config.AssemblyPath == "./MyService.dll");
                because.ItsTrue("InterfaceTypeName is parsed", config.InterfaceTypeName == "My.Ns.IMyService");
                because.ItsTrue("ImplementationTypeName is parsed", config.ImplementationTypeName == "My.Ns.MyService");
                because.ItsTrue("ImplementationAssemblyPath is parsed", config.ImplementationAssemblyPath == "./MyService.Impl.dll");
                because.ItsTrue("OutputDirectory is parsed", config.OutputDirectory == "./out");
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void ReadTheStarterConfigItWrites()
        {
            string dir = TestOutputDir("starter");
            Directory.CreateDirectory(dir);
            string yamlPath = Path.Combine(dir, "starter.yaml");
            File.WriteAllText(yamlPath, GenerateMenuContainer.DecoratorConfigTemplate);

            After.Setup(reg =>
            {
                reg.For<FileInfo>().Use(new FileInfo(yamlPath));
            })
            .When<FileInfo>("reads the starter config written by decorator-init", file =>
            {
                return BamDecoratorGenerationConfig.ReadFrom(file);
            })
            .TheTest
            .ShouldPass<BamDecoratorGenerationConfig>((because, config) =>
            {
                because.ItsTrue("the placeholders are parsed", config.InterfaceTypeName == "My.Namespace.IMyService" && config.ImplementationTypeName == "My.Namespace.MyService");
                because.ItsTrue("the empty optional field is not set", string.IsNullOrWhiteSpace(config.ImplementationAssemblyPath));
                because.ItsTrue("the output directory is parsed", config.OutputDirectory == "./Generated_Decorators");
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void GenerateADecoratorToDisk()
        {
            string outDir = TestOutputDir("generate");
            if (Directory.Exists(outDir))
            {
                Directory.Delete(outDir, true);
            }

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("generates a decorator file", command =>
            {
                string path = command.Execute(ValidConfig(outDir));
                return new GenerationOutcome(path, File.Exists(path) ? File.ReadAllText(path) : string.Empty);
            })
            .TheTest
            .ShouldPass<GenerationOutcome>((because, outcome) =>
            {
                because.ItsTrue("returns the GreetingServiceDecorator.cs path", outcome.Path == Path.Combine(outDir, "GreetingServiceDecorator.cs"), outcome.Path);
                because.ItsTrue("wrote the decorator class", outcome.Source.Contains("public partial class GreetingServiceDecorator"));
                because.ItsTrue("the decorator implements the interface", outcome.Source.Contains(", global::Bam.Tests.TestClasses.IGreetingService"));
                because.ItsTrue("wrote the typed extension methods", outcome.Source.Contains(" OnGreetStart(") && outcome.Source.Contains(" OnGreetAsyncError("));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void LoadTheImplementationFromItsOwnAssembly()
        {
            string outDir = TestOutputDir("two-assemblies");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given a separate implementation assembly", command =>
            {
                BamDecoratorGenerationConfig config = ValidConfig(outDir);
                config.ImplementationAssemblyPath = typeof(GreetingService).Assembly.Location;
                string path = command.Execute(config);

                config.ImplementationAssemblyPath = Path.Combine(outDir, "missing.dll");
                return new RejectionOutcome(File.Exists(path), Thrown(() => command.Execute(config)));
            })
            .TheTest
            .ShouldPass<RejectionOutcome>((because, outcome) =>
            {
                because.ItsTrue("generates from the named implementation assembly", outcome.Succeeded);
                because.ItsTrue("throws FileNotFoundException when that assembly is missing", outcome.Thrown is FileNotFoundException);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void RejectConfigWithMissingRequiredFields()
        {
            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given configs with a required field missing", command =>
            {
                string outDir = TestOutputDir("missing-fields");
                BamDecoratorGenerationConfig noAssembly = ValidConfig(outDir);
                noAssembly.AssemblyPath = string.Empty;
                BamDecoratorGenerationConfig noInterface = ValidConfig(outDir);
                noInterface.InterfaceTypeName = " ";
                BamDecoratorGenerationConfig noImplementation = ValidConfig(outDir);
                noImplementation.ImplementationTypeName = null!;
                BamDecoratorGenerationConfig noOutput = ValidConfig(outDir);
                noOutput.OutputDirectory = string.Empty;

                return new MissingFieldOutcome(
                    Thrown(() => command.Execute(noAssembly)),
                    Thrown(() => command.Execute(noInterface)),
                    Thrown(() => command.Execute(noImplementation)),
                    Thrown(() => command.Execute(noOutput)),
                    Thrown(() => command.Execute(null!)));
            })
            .TheTest
            .ShouldPass<MissingFieldOutcome>((because, outcome) =>
            {
                because.ItsTrue("throws ArgumentException when AssemblyPath is missing", outcome.NoAssembly is ArgumentException);
                because.ItsTrue("throws ArgumentException when InterfaceTypeName is missing", outcome.NoInterface is ArgumentException);
                because.ItsTrue("throws ArgumentException when ImplementationTypeName is missing", outcome.NoImplementation is ArgumentException);
                because.ItsTrue("throws ArgumentException when OutputDirectory is missing", outcome.NoOutput is ArgumentException);
                because.ItsTrue("throws ArgumentNullException for a null config", outcome.NullConfig is ArgumentNullException);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void RejectWhatItCannotResolveOrDecorate()
        {
            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given an unknown assembly, unknown types and a mismatched pair", command =>
            {
                string outDir = TestOutputDir("unresolved");
                BamDecoratorGenerationConfig missingAssembly = ValidConfig(outDir);
                missingAssembly.AssemblyPath = Path.Combine(outDir, "missing.dll");
                BamDecoratorGenerationConfig unknownInterface = ValidConfig(outDir);
                unknownInterface.InterfaceTypeName = "No.Such.IService";
                BamDecoratorGenerationConfig unknownImplementation = ValidConfig(outDir);
                unknownImplementation.ImplementationTypeName = "No.Such.Service";
                BamDecoratorGenerationConfig mismatched = ValidConfig(outDir);
                mismatched.ImplementationTypeName = typeof(NotAGreetingService).FullName!;

                return new UnresolvedOutcome(
                    Thrown(() => command.Execute(missingAssembly)),
                    Thrown(() => command.Execute(unknownInterface)),
                    Thrown(() => command.Execute(unknownImplementation)),
                    Thrown(() => command.Execute(mismatched)),
                    File.Exists(Path.Combine(outDir, "NotAGreetingServiceDecorator.cs")));
            })
            .TheTest
            .ShouldPass<UnresolvedOutcome>((because, outcome) =>
            {
                because.ItsTrue("throws FileNotFoundException for a missing assembly", outcome.MissingAssembly is FileNotFoundException);
                because.ItsTrue("throws ArgumentException naming an unknown interface", outcome.UnknownInterface is ArgumentException && outcome.UnknownInterface.Message.Contains("No.Such.IService"));
                because.ItsTrue("throws ArgumentException naming an unknown implementation", outcome.UnknownImplementation is ArgumentException && outcome.UnknownImplementation.Message.Contains("No.Such.Service"));
                because.ItsTrue("throws DecoratorGenerationException for an implementation that does not implement the interface", outcome.Mismatched is DecoratorGenerationException);
                because.ItsTrue("writes nothing for the rejected pair", !outcome.WroteRejectedPair);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        private static Exception? Thrown(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private sealed record GenerationOutcome(string Path, string Source);

        private sealed record RejectionOutcome(bool Succeeded, Exception? Thrown);

        private sealed record MissingFieldOutcome(Exception? NoAssembly, Exception? NoInterface, Exception? NoImplementation, Exception? NoOutput, Exception? NullConfig);

        private sealed record UnresolvedOutcome(Exception? MissingAssembly, Exception? UnknownInterface, Exception? UnknownImplementation, Exception? Mismatched, bool WroteRejectedPair);
    }
}
