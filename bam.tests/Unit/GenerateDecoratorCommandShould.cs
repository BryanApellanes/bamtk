using Bam.DependencyInjection;
using Bam.Generators.Decorators;
using Bam.Test;
using Bam.Tests.TestClasses;
using Bam.Tests.TestClasses.Decorated;
using Microsoft.CodeAnalysis;

namespace Bam.Tests.Unit
{
    [UnitTestMenu("GenerateDecoratorCommand should")]
    public class GenerateDecoratorCommandShould : UnitTestMenuContainer
    {
        public GenerateDecoratorCommandShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        // A fresh directory per test: whatever a previous run left there is removed first, so leftovers never
        // build up past one run's worth. The process id keeps two runs at once (two worktrees, say) out of each
        // other's directories.
        private static string TestOutputDir(string name)
        {
            string dir = Path.Combine(Path.GetTempPath(), "bam-decorator-tests", Environment.ProcessId.ToString(), name);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            return dir;
        }

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

        private static BamDecoratorGenerationConfig ListConfig(string outputDirectory, params DecoratorServiceConfig[] services)
        {
            BamDecoratorGenerationConfig config = new BamDecoratorGenerationConfig
            {
                AssemblyPath = typeof(IGreetingService).Assembly.Location,
                OutputDirectory = outputDirectory
            };
            config.Services.AddRange(services);
            return config;
        }

        private static DecoratorServiceConfig Pair(Type interfaceType, Type? implementationType)
        {
            return new DecoratorServiceConfig
            {
                Interface = interfaceType.FullName!,
                Implementation = implementationType?.FullName
            };
        }

        private static string FileNames(IEnumerable<string> paths)
        {
            return string.Join(",", paths.Select(Path.GetFileName));
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
                "OutputDirectory: ./out\n" +
                "RegistryExtensions: false\n" +
                "TemplateDirectory: ./Templates/Custom\n" +
                "ProbeDirectories:\n" +
                "  - ./lib\n" +
                "  - ../shared/bin\n" +
                "Services:\n" +
                "  - Interface: My.Ns.IOther\n" +
                "    Implementation: My.Ns.Other\n" +
                "    AssemblyPath: ./Other.dll\n" +
                "  - Interface: My.Ns.IAlone\n" +
                "Discover:\n" +
                "  Namespace: My.Ns.Found\n" +
                "  InterfaceOnly: true\n");

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
                because.ItsTrue("RegistryExtensions is parsed", !config.RegistryExtensions);
                because.ItsTrue("TemplateDirectory is parsed", config.TemplateDirectory == "./Templates/Custom");
                because.ItsTrue("ProbeDirectories are parsed", config.ProbeDirectories.Count == 2 && config.ProbeDirectories[1] == "../shared/bin");
                because.ItsTrue("Services are parsed", config.Services.Count == 2 && config.Services[0].Interface == "My.Ns.IOther" && config.Services[0].Implementation == "My.Ns.Other" && config.Services[0].AssemblyPath == "./Other.dll");
                because.ItsTrue("a service without an implementation is parsed", config.Services[1].Interface == "My.Ns.IAlone" && config.Services[1].Implementation is null);
                because.ItsTrue("Discover is parsed", config.Discover is not null && config.Discover.Namespace == "My.Ns.Found" && config.Discover.InterfaceOnly && config.Discover.AssemblyPath is null);
                because.ItsTrue("GetServices lists the single pair first, then the list", config.GetServices().Count == 3 && config.GetServices()[0].Interface == "My.Ns.IMyService" && config.GetServices()[2].Interface == "My.Ns.IAlone");
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
                because.ItsTrue("registry extensions default on", config.RegistryExtensions);
                because.ItsTrue("the template directory is not set", string.IsNullOrWhiteSpace(config.TemplateDirectory));
                because.ItsTrue("the probe directories are empty", config.ProbeDirectories is not null && config.ProbeDirectories.Count == 0);
                because.ItsTrue("the services list is empty", config.Services is not null && config.Services.Count == 0);
                because.ItsTrue("discovery is not set (the example is commented out)", config.Discover is null);
                because.ItsTrue("the single pair is the only service", config.GetServices().Count == 1);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void ReadTheStarterConfigWithItsExamplesUncommented()
        {
            string dir = TestOutputDir("starter-uncommented");
            Directory.CreateDirectory(dir);
            string yamlPath = Path.Combine(dir, "starter.yaml");
            // Every commented line after the title is an example the user is invited to uncomment.
            string[] lines = GenerateMenuContainer.DecoratorConfigTemplate.Split('\n');
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#", StringComparison.Ordinal))
                {
                    lines[i] = lines[i].Substring(1);
                }
            }

            File.WriteAllText(yamlPath, string.Join("\n", lines));

            After.Setup(reg =>
            {
                reg.For<FileInfo>().Use(new FileInfo(yamlPath));
            })
            .When<FileInfo>("reads the starter config with its examples uncommented", file =>
            {
                return BamDecoratorGenerationConfig.ReadFrom(file);
            })
            .TheTest
            .ShouldPass<BamDecoratorGenerationConfig>((because, config) =>
            {
                because.ItsTrue("the example service is parsed", config.Services.Count == 1 && config.Services[0].Interface == "My.Namespace.IOtherService" && config.Services[0].Implementation == "My.Namespace.OtherService", config.Services.Count.ToString());
                because.ItsTrue("its blank assembly paths are not set", string.IsNullOrWhiteSpace(config.Services[0].AssemblyPath) && string.IsNullOrWhiteSpace(config.Services[0].ImplementationAssemblyPath));
                because.ItsTrue("the example discovery is parsed", config.Discover is not null && config.Discover.Namespace == "My.Namespace" && !config.Discover.InterfaceOnly && string.IsNullOrWhiteSpace(config.Discover.AssemblyPath));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void GenerateADecoratorToDisk()
        {
            string outDir = TestOutputDir("generate");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("generates a decorator file", command =>
            {
                DecoratorGenerationReport report = command.Execute(ValidConfig(outDir));
                string path = report.Written.Count == 1 ? report.Written[0] : string.Empty;
                return new GenerationOutcome(report, path, File.Exists(path) ? File.ReadAllText(path) : string.Empty);
            })
            .TheTest
            .ShouldPass<GenerationOutcome>((because, outcome) =>
            {
                because.ItsTrue("reports the GreetingServiceDecorator.cs path", outcome.Path == Path.Combine(outDir, "GreetingServiceDecorator.cs"), outcome.Path);
                because.ItsTrue("skips nothing", outcome.Report.Skipped.Count == 0 && outcome.Report.WroteAnything);
                because.ItsTrue("wrote the decorator class", outcome.Source.Contains("public partial class GreetingServiceDecorator"));
                because.ItsTrue("the decorator implements the interface", outcome.Source.Contains(", global::Bam.Tests.TestClasses.IGreetingService"));
                because.ItsTrue("wrote the typed extension methods", outcome.Source.Contains(" OnGreetStart(") && outcome.Source.Contains(" OnGreetAsyncError("));
                because.ItsTrue("wrote the registry extensions by default", outcome.Source.Contains("ServiceRegistry"));
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
            .When<GenerateDecoratorCommand>("is given an interface and an implementation from different assemblies", command =>
            {
                // ITemplateRenderer lives in bam.base; HandlebarsTemplateRenderer in bam.generators. Only the
                // implementation assembly path can find the implementation.
                BamDecoratorGenerationConfig config = new BamDecoratorGenerationConfig
                {
                    AssemblyPath = typeof(ITemplateRenderer).Assembly.Location,
                    InterfaceTypeName = typeof(ITemplateRenderer).FullName!,
                    ImplementationTypeName = typeof(Bam.Generators.HandlebarsTemplateRenderer).FullName!,
                    OutputDirectory = outDir
                };

                DecoratorGenerationReport withoutPath = command.Execute(config);

                config.ImplementationAssemblyPath = typeof(Bam.Generators.HandlebarsTemplateRenderer).Assembly.Location;
                DecoratorGenerationReport withPath = command.Execute(config);

                config.ImplementationAssemblyPath = Path.Combine(outDir, "missing.dll");
                return new TwoAssemblyOutcome(withoutPath, withPath.Written.Count == 1 && File.Exists(withPath.Written[0]), Thrown(() => command.Execute(config)));
            })
            .TheTest
            .ShouldPass<TwoAssemblyOutcome>((because, outcome) =>
            {
                because.ItsTrue("without the implementation assembly, the pair is skipped naming the implementation", outcome.WithoutImplementationPath.Written.Count == 0 && outcome.WithoutImplementationPath.Skipped.Count == 1 && outcome.WithoutImplementationPath.Skipped[0].Reason.Contains("HandlebarsTemplateRenderer"), outcome.WithoutImplementationPath.Skipped.FirstOrDefault()?.ToString());
                because.ItsTrue("with it, the decorator is generated", outcome.Generated);
                because.ItsTrue("a missing implementation assembly throws FileNotFoundException", outcome.MissingAssembly is FileNotFoundException, outcome.MissingAssembly?.GetType().Name);
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
                BamDecoratorGenerationConfig listedWithoutInterface = ListConfig(outDir, new DecoratorServiceConfig { Interface = string.Empty, Implementation = typeof(GreetingService).FullName });
                BamDecoratorGenerationConfig listedWithoutAssembly = ListConfig(outDir, Pair(typeof(IGreetingService), typeof(GreetingService)));
                listedWithoutAssembly.AssemblyPath = null!;

                return new MissingFieldOutcome(
                    Thrown(() => command.Execute(noAssembly)),
                    Thrown(() => command.Execute(noInterface)),
                    command.Execute(noImplementation),
                    Thrown(() => command.Execute(noOutput)),
                    Thrown(() => command.Execute(null!)),
                    Thrown(() => command.Execute(listedWithoutInterface)),
                    Thrown(() => command.Execute(listedWithoutAssembly)));
            })
            .TheTest
            .ShouldPass<MissingFieldOutcome>((because, outcome) =>
            {
                because.ItsTrue("throws ArgumentException when AssemblyPath is missing", outcome.NoAssembly is ArgumentException);
                because.ItsTrue("throws ArgumentException when no service is configured at all", outcome.NoInterface is ArgumentException && outcome.NoInterface.Message.Contains("No service"), outcome.NoInterface?.Message);
                because.ItsTrue("decorates the interface alone when ImplementationTypeName is missing", outcome.NoImplementation.Written.Count == 1 && Path.GetFileName(outcome.NoImplementation.Written[0]) == "IGreetingServiceDecorator.cs", FileNames(outcome.NoImplementation.Written));
                because.ItsTrue("throws ArgumentException when OutputDirectory is missing", outcome.NoOutput is ArgumentException);
                because.ItsTrue("throws ArgumentNullException for a null config", outcome.NullConfig is ArgumentNullException);
                because.ItsTrue("throws ArgumentException naming the entry when a listed service has no interface", outcome.ListedWithoutInterface is ArgumentException && outcome.ListedWithoutInterface.Message.Contains("Services[0].Interface"), outcome.ListedWithoutInterface?.Message);
                because.ItsTrue("throws ArgumentException when a listed service has no assembly anywhere", outcome.ListedWithoutAssembly is ArgumentException && outcome.ListedWithoutAssembly.Message.Contains("AssemblyPath"), outcome.ListedWithoutAssembly?.Message);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void SkipWhatItCannotResolveOrDecorate()
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
                    command.Execute(unknownInterface),
                    command.Execute(unknownImplementation),
                    command.Execute(mismatched),
                    File.Exists(Path.Combine(outDir, "NotAGreetingServiceDecorator.cs")));
            })
            .TheTest
            .ShouldPass<UnresolvedOutcome>((because, outcome) =>
            {
                because.ItsTrue("throws FileNotFoundException for a missing assembly", outcome.MissingAssembly is FileNotFoundException);
                because.ItsTrue("skips an unknown interface, naming it", OnlySkip(outcome.UnknownInterface)?.Reason.Contains("No.Such.IService") == true, OnlySkip(outcome.UnknownInterface)?.ToString());
                because.ItsTrue("skips an unknown implementation, naming it", OnlySkip(outcome.UnknownImplementation)?.Reason.Contains("No.Such.Service") == true, OnlySkip(outcome.UnknownImplementation)?.ToString());
                because.ItsTrue("skips an implementation that does not implement the interface, saying so", OnlySkip(outcome.Mismatched)?.Reason.Contains("does not implement") == true, OnlySkip(outcome.Mismatched)?.ToString());
                because.ItsTrue("the skip names the pair", OnlySkip(outcome.Mismatched)?.InterfaceName == typeof(IGreetingService).FullName && OnlySkip(outcome.Mismatched)?.ImplementationName == typeof(NotAGreetingService).FullName);
                because.ItsTrue("writes nothing for the rejected pair", !outcome.WroteRejectedPair);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void GenerateEveryListedService()
        {
            string outDir = TestOutputDir("listed");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given the single pair plus a Services list", command =>
            {
                BamDecoratorGenerationConfig config = ValidConfig(outDir);
                config.Services.Add(Pair(typeof(IFarewellService), typeof(FarewellService)));
                config.Services.Add(Pair(typeof(IUnimplementedService), null));
                return command.Execute(config);
            })
            .TheTest
            .ShouldPass<DecoratorGenerationReport>((because, report) =>
            {
                because.ItsTrue("writes one file per service, in config order", FileNames(report.Written) == "GreetingServiceDecorator.cs,FarewellServiceDecorator.cs,IUnimplementedServiceDecorator.cs", FileNames(report.Written));
                because.ItsTrue("every file exists", report.Written.All(File.Exists));
                because.ItsTrue("skips nothing", report.Skipped.Count == 0, string.Join("; ", report.Skipped));
                because.ItsTrue("the interface-only decorator is typed on the interface", File.ReadAllText(report.Written[2]).Contains("public partial class IUnimplementedServiceDecorator"));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void ContinuePastAServiceItCannotGenerate()
        {
            string outDir = TestOutputDir("continue");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given a bad pair between two good ones, and a pair listed twice", command =>
            {
                BamDecoratorGenerationConfig config = ListConfig(outDir,
                    Pair(typeof(IGreetingService), typeof(GreetingService)),
                    Pair(typeof(IGreetingService), typeof(NotAGreetingService)),
                    new DecoratorServiceConfig { Interface = "No.Such.IService" },
                    Pair(typeof(IFarewellService), typeof(FarewellService)),
                    Pair(typeof(IGreetingService), typeof(GreetingService)));
                DecoratorGenerationReport report = command.Execute(config);
                return new ContinueOutcome(report, Directory.GetFiles(outDir).Length);
            })
            .TheTest
            .ShouldPass<ContinueOutcome>((because, outcome) =>
            {
                because.ItsTrue("writes the good pairs", FileNames(outcome.Report.Written) == "GreetingServiceDecorator.cs,FarewellServiceDecorator.cs", FileNames(outcome.Report.Written));
                because.ItsTrue("skips the three bad entries", outcome.Report.Skipped.Count == 3, string.Join("; ", outcome.Report.Skipped));
                because.ItsTrue("the mismatched pair is skipped with the generator's reason", outcome.Report.Skipped[0].ImplementationName == typeof(NotAGreetingService).FullName && outcome.Report.Skipped[0].Reason.Contains("does not implement"));
                because.ItsTrue("the unknown interface is skipped by name", outcome.Report.Skipped[1].InterfaceName == "No.Such.IService" && outcome.Report.Skipped[1].Reason.Contains("not found"));
                because.ItsTrue("the repeated pair is skipped rather than overwritten", outcome.Report.Skipped[2].Reason.Contains("already wrote"), outcome.Report.Skipped[2].Reason);
                because.ItsTrue("only the two files are on disk", outcome.FilesOnDisk == 2);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void SkipAServiceThatWouldOverwriteAnotherNamespacesFile()
        {
            string outDir = TestOutputDir("samefile");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given two same-named interfaces from different namespaces", command =>
            {
                BamDecoratorGenerationConfig config = ListConfig(outDir,
                    Pair(typeof(Bam.Tests.TestClasses.SameFile.Billing.IClient), null),
                    Pair(typeof(Bam.Tests.TestClasses.SameFile.Shipping.IClient), null));
                DecoratorGenerationReport report = command.Execute(config);
                string source = report.Written.Count == 1 ? File.ReadAllText(report.Written[0]) : string.Empty;
                return new GenerationOutcome(report, Directory.GetFiles(outDir).Length.ToString(), source);
            })
            .TheTest
            .ShouldPass<GenerationOutcome>((because, outcome) =>
            {
                because.ItsTrue("writes the first", FileNames(outcome.Report.Written) == "IClientDecorator.cs", FileNames(outcome.Report.Written));
                because.ItsTrue("skips the second", outcome.Report.Skipped.Count == 1 && outcome.Report.Skipped[0].InterfaceName == typeof(Bam.Tests.TestClasses.SameFile.Shipping.IClient).FullName, string.Join("; ", outcome.Report.Skipped));
                because.ItsTrue("the reason names the file and the decorator already in it", outcome.Report.Skipped.Count == 1 && outcome.Report.Skipped[0].Reason.Contains("IClientDecorator.cs") && outcome.Report.Skipped[0].Reason.Contains("Billing") && outcome.Report.Skipped[0].Reason.Contains("overwrite"), outcome.Report.Skipped.Count == 1 ? outcome.Report.Skipped[0].Reason : "no skip");
                because.ItsTrue("one file is on disk", outcome.Path == "1", outcome.Path);
                because.ItsTrue("and it still holds the first decorator", outcome.Source.Contains("SameFile.Billing.IClient") && !outcome.Source.Contains("SameFile.Shipping.IClient"));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void FindAMissingAssemblyBeforeWritingAnything()
        {
            string listedDir = TestOutputDir("missing-later-listed");
            string discoverDir = TestOutputDir("missing-later-discover");
            string probeDir = TestOutputDir("missing-later-probe");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given a good pair followed by something that names a missing path", command =>
            {
                BamDecoratorGenerationConfig laterEntry = ListConfig(listedDir,
                    Pair(typeof(IGreetingService), typeof(GreetingService)),
                    new DecoratorServiceConfig { Interface = typeof(IFarewellService).FullName!, AssemblyPath = Path.Combine(listedDir, "missing.dll") });
                Exception? listed = Thrown(() => command.Execute(laterEntry));

                BamDecoratorGenerationConfig discovery = ListConfig(discoverDir, Pair(typeof(IGreetingService), typeof(GreetingService)));
                discovery.Discover = new DecoratorDiscoveryConfig { AssemblyPath = Path.Combine(discoverDir, "missing.dll") };
                Exception? discovered = Thrown(() => command.Execute(discovery));

                BamDecoratorGenerationConfig probing = ListConfig(probeDir, Pair(typeof(IGreetingService), typeof(GreetingService)));
                probing.ProbeDirectories.Add(Path.Combine(probeDir, "no-such-directory"));
                Exception? probed = Thrown(() => command.Execute(probing));

                return new MissingPathOutcome(listed, discovered, probed, Directory.Exists(listedDir) || Directory.Exists(discoverDir) || Directory.Exists(probeDir));
            })
            .TheTest
            .ShouldPass<MissingPathOutcome>((because, outcome) =>
            {
                because.ItsTrue("a later entry's missing assembly throws FileNotFoundException", outcome.LaterEntry is FileNotFoundException, outcome.LaterEntry?.GetType().Name);
                because.ItsTrue("a missing discovery assembly throws FileNotFoundException", outcome.Discovery is FileNotFoundException, outcome.Discovery?.GetType().Name);
                because.ItsTrue("a probe directory that does not exist throws DirectoryNotFoundException naming it", outcome.ProbeDirectory is DirectoryNotFoundException && outcome.ProbeDirectory.Message.Contains("no-such-directory"), outcome.ProbeDirectory?.Message);
                because.ItsTrue("nothing was written before any of them", !outcome.WroteAnything);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void SkipAnAssemblyItCannotLoadAndCarryOn()
        {
            string outDir = TestOutputDir("bad-image");
            string badDir = TestOutputDir("bad-image-src");
            Directory.CreateDirectory(badDir);
            string notAnAssembly = Path.Combine(badDir, "NotAnAssembly.dll");
            File.WriteAllText(notAnAssembly, "this is not an assembly");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given a file that is not an assembly between two good pairs", command =>
            {
                BamDecoratorGenerationConfig config = ListConfig(outDir,
                    Pair(typeof(IGreetingService), typeof(GreetingService)),
                    new DecoratorServiceConfig { Interface = "Some.IService", AssemblyPath = notAnAssembly },
                    new DecoratorServiceConfig { Interface = typeof(IFarewellService).FullName!, Implementation = typeof(FarewellService).FullName, AssemblyPath = " " });
                config.Discover = new DecoratorDiscoveryConfig { AssemblyPath = notAnAssembly };
                return command.Execute(config);
            })
            .TheTest
            .ShouldPass<DecoratorGenerationReport>((because, report) =>
            {
                because.ItsTrue("writes the pairs around it (a blank AssemblyPath on an entry falls back to the top-level one)", FileNames(report.Written) == "GreetingServiceDecorator.cs,FarewellServiceDecorator.cs", FileNames(report.Written));
                because.ItsTrue("skips the entry, saying the assembly could not be loaded", report.Skipped.Count == 2 && report.Skipped[0].InterfaceName == "Some.IService" && report.Skipped[0].Reason.Contains("could not be loaded"), string.Join("; ", report.Skipped));
                because.ItsTrue("skips the discovery scan of it, naming the file", report.Skipped.Count == 2 && report.Skipped[1].InterfaceName == notAnAssembly && report.Skipped[1].Reason.Contains("was not scanned"), string.Join("; ", report.Skipped));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void LeaveListedInterfacesOutOfDiscovery()
        {
            string outDir = TestOutputDir("listed-and-discovered");
            string interfaceOnlyDir = TestOutputDir("listed-and-discovered-interface-only");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given Services entries and a Discover over the same namespace", command =>
            {
                BamDecoratorGenerationConfig config = ListConfig(outDir,
                    Pair(typeof(IAmbiguousService), typeof(AmbiguousServiceA)),
                    Pair(typeof(IFarewellService), typeof(FarewellService)));
                config.Discover = new DecoratorDiscoveryConfig { Namespace = typeof(IFarewellService).Namespace };
                DecoratorGenerationReport paired = command.Execute(config);

                config.OutputDirectory = interfaceOnlyDir;
                config.Discover.InterfaceOnly = true;
                DecoratorGenerationReport interfaceOnly = command.Execute(config);
                return new DiscoveryOutcome(paired, interfaceOnly);
            })
            .TheTest
            .ShouldPass<DiscoveryOutcome>((because, outcome) =>
            {
                because.ItsTrue("writes the listed pairs once", FileNames(outcome.Paired.Written) == "AmbiguousServiceADecorator.cs,FarewellServiceDecorator.cs", FileNames(outcome.Paired.Written));
                because.ItsTrue("discovery reports only the interface nobody listed", outcome.Paired.Skipped.Count == 1 && outcome.Paired.Skipped[0].InterfaceName == typeof(IUnimplementedService).FullName, string.Join("; ", outcome.Paired.Skipped));
                because.ItsTrue("with InterfaceOnly, discovery adds only the unlisted interface", FileNames(outcome.InterfaceOnly.Written) == "AmbiguousServiceADecorator.cs,FarewellServiceDecorator.cs,IUnimplementedServiceDecorator.cs" && outcome.InterfaceOnly.Skipped.Count == 0, FileNames(outcome.InterfaceOnly.Written) + " " + string.Join("; ", outcome.InterfaceOnly.Skipped));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void SayWhenDiscoveryMatchesNothing()
        {
            string outDir = TestOutputDir("discover-nothing");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("discovers a namespace nothing lives in", command =>
            {
                BamDecoratorGenerationConfig config = new BamDecoratorGenerationConfig
                {
                    AssemblyPath = typeof(IFarewellService).Assembly.Location,
                    OutputDirectory = outDir,
                    Discover = new DecoratorDiscoveryConfig { Namespace = "No.Such.Namespace" }
                };
                return command.Execute(config);
            })
            .TheTest
            .ShouldPass<DecoratorGenerationReport>((because, report) =>
            {
                because.ItsTrue("writes nothing", report.Written.Count == 0);
                because.ItsTrue("reports the namespace it found nothing in", report.Skipped.Count == 1 && report.Skipped[0].Reason.Contains("No.Such.Namespace") && report.Skipped[0].Reason.Contains("no public interface"), string.Join("; ", report.Skipped));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void FailTheMenuCommandWhenNothingIsWritten()
        {
            string outDir = TestOutputDir("menu");

            After.Setup(reg =>
            {
                reg.For<GenerateMenuContainer>().Use(new GenerateMenuContainer(new ServiceRegistry()));
            })
            .When<GenerateMenuContainer>("runs the decorator command", menu =>
            {
                BamDecoratorGenerationConfig nothing = ValidConfig(outDir);
                nothing.InterfaceTypeName = "No.Such.IService";
                Exception? failed = Thrown(() => menu.Decorator(nothing));
                Exception? succeeded = Thrown(() => menu.Decorator(ValidConfig(outDir)));
                return new MenuOutcome(failed, succeeded, File.Exists(Path.Combine(outDir, "GreetingServiceDecorator.cs")));
            })
            .TheTest
            .ShouldPass<MenuOutcome>((because, outcome) =>
            {
                because.ItsTrue("throws when every service was skipped", outcome.NothingWritten is InvalidOperationException && outcome.NothingWritten.Message.Contains("No decorator was generated"), outcome.NothingWritten?.Message);
                because.ItsTrue("the message counts the skips without repeating them", outcome.NothingWritten is not null && outcome.NothingWritten.Message.Contains("1 skipped") && !outcome.NothingWritten.Message.Contains("No.Such.IService"), outcome.NothingWritten?.Message);
                because.ItsTrue("does not throw when a decorator was written", outcome.Written is null && outcome.FileExists, outcome.Written?.Message);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void DiscoverServicesInAnAssembly()
        {
            string outDir = TestOutputDir("discover");
            string interfaceOnlyDir = TestOutputDir("discover-interface-only");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("discovers the services of a namespace", command =>
            {
                BamDecoratorGenerationConfig config = new BamDecoratorGenerationConfig
                {
                    AssemblyPath = typeof(IFarewellService).Assembly.Location,
                    OutputDirectory = outDir,
                    Discover = new DecoratorDiscoveryConfig { Namespace = typeof(IFarewellService).Namespace }
                };
                DecoratorGenerationReport paired = command.Execute(config);

                config.OutputDirectory = interfaceOnlyDir;
                config.Discover.InterfaceOnly = true;
                DecoratorGenerationReport interfaceOnly = command.Execute(config);
                return new DiscoveryOutcome(paired, interfaceOnly);
            })
            .TheTest
            .ShouldPass<DiscoveryOutcome>((because, outcome) =>
            {
                // The scanned namespace holds a compiled FarewellServiceDecorator and IUnimplementedServiceDecorator
                // (DecoratorDiscoveryFixtures.cs), as a project does once it compiles what a first run generated.
                because.ItsTrue("pairs an interface with its single public implementation, not counting its compiled decorator", FileNames(outcome.Paired.Written) == "FarewellServiceDecorator.cs", FileNames(outcome.Paired.Written) + " " + string.Join("; ", outcome.Paired.Skipped));
                because.ItsTrue("never decorates a decorator", !outcome.Paired.Written.Concat(outcome.InterfaceOnly.Written).Any(path => path.Contains("DecoratorDecorator")), FileNames(outcome.Paired.Written.Concat(outcome.InterfaceOnly.Written)));
                because.ItsTrue("skips the interface with two implementations, naming them", outcome.Paired.Skipped.Any(skip => skip.InterfaceName == typeof(IAmbiguousService).FullName && skip.Reason.Contains("2 public implementations") && skip.Reason.Contains(nameof(AmbiguousServiceB))), string.Join("; ", outcome.Paired.Skipped));
                because.ItsTrue("skips the interface with no public implementation", outcome.Paired.Skipped.Any(skip => skip.InterfaceName == typeof(IUnimplementedService).FullName && skip.Reason.Contains("no public implementation")));
                because.ItsTrue("nothing else is skipped", outcome.Paired.Skipped.Count == 2);
                because.ItsTrue("stays inside the namespace", !outcome.Paired.Written.Concat(outcome.Paired.Skipped.Select(skip => skip.InterfaceName)).Any(name => name.Contains(nameof(IGreetingService))));
                because.ItsTrue("with InterfaceOnly, the rest are decorated from the interface alone", FileNames(outcome.InterfaceOnly.Written) == "IAmbiguousServiceDecorator.cs,FarewellServiceDecorator.cs,IUnimplementedServiceDecorator.cs" && outcome.InterfaceOnly.Skipped.Count == 0, FileNames(outcome.InterfaceOnly.Written));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void GenerateWithoutRegistryExtensions()
        {
            string outDir = TestOutputDir("no-registry");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is told not to generate registry extensions", command =>
            {
                BamDecoratorGenerationConfig config = ValidConfig(outDir);
                config.RegistryExtensions = false;
                DecoratorGenerationReport report = command.Execute(config);
                return new GenerationOutcome(report, report.Written[0], File.ReadAllText(report.Written[0]));
            })
            .TheTest
            .ShouldPass<GenerationOutcome>((because, outcome) =>
            {
                because.ItsTrue("writes the decorator", outcome.Source.Contains("public partial class GreetingServiceDecorator"));
                because.ItsTrue("keeps the instance hooks", outcome.Source.Contains(" OnGreetStart("));
                because.ItsTrue("names no ServiceRegistry", !outcome.Source.Contains("ServiceRegistry"));
                because.ItsTrue("names no Bam.Logging", !outcome.Source.Contains("Bam.Logging"));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void UseTheConfiguredTemplateDirectory()
        {
            string outDir = TestOutputDir("templates-out");
            string templateDir = TestOutputDir("templates");
            Directory.CreateDirectory(templateDir);
            File.WriteAllText(Path.Combine(templateDir, "Decorator.hbs"), "// custom template for {{DecoratorTypeName}}\n");

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given a template directory", command =>
            {
                BamDecoratorGenerationConfig config = ValidConfig(outDir);
                config.TemplateDirectory = templateDir;
                DecoratorGenerationReport report = command.Execute(config);
                string source = File.ReadAllText(report.Written[0]);

                config.TemplateDirectory = Path.Combine(templateDir, "missing");
                return new TemplateOutcome(source, Thrown(() => command.Execute(config)));
            })
            .TheTest
            .ShouldPass<TemplateOutcome>((because, outcome) =>
            {
                because.ItsTrue("renders the override template", outcome.Source.Trim() == "// custom template for GreetingServiceDecorator", outcome.Source);
                because.ItsTrue("a template directory that does not exist throws DirectoryNotFoundException", outcome.Missing is DirectoryNotFoundException, outcome.Missing?.GetType().Name);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void ProbeForDependencies()
        {
            string root = TestOutputDir("probe");
            string outDir = Path.Combine(root, "out");
            string tag = Guid.NewGuid().ToString("N").Substring(0, 8);

            // Two independent service assemblies, each depending on its own dependency assembly that lives in a
            // probe directory, away from the service. The runtime finds a dependency next to the assembly that
            // needs it; these are deliberately elsewhere.
            CompiledService found = CompileService(root, "Found" + tag, false);
            CompiledService lost = CompileService(root, "Lost" + tag, false);
            // The same again, with the service interface itself deriving from an interface in the dependency: here
            // the type cannot even be resolved without the dependency.
            CompiledService foundBase = CompileService(root, "FoundBase" + tag, true);
            CompiledService lostBase = CompileService(root, "LostBase" + tag, true);

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given a service whose dependency lives in a probe directory", command =>
            {
                BamDecoratorGenerationConfig without = new BamDecoratorGenerationConfig
                {
                    AssemblyPath = lost.ServicePath,
                    InterfaceTypeName = lost.InterfaceName,
                    ImplementationTypeName = lost.ImplementationName,
                    OutputDirectory = outDir
                };
                DecoratorGenerationReport withoutProbe = command.Execute(without);

                BamDecoratorGenerationConfig with = new BamDecoratorGenerationConfig
                {
                    AssemblyPath = found.ServicePath,
                    InterfaceTypeName = found.InterfaceName,
                    ImplementationTypeName = found.ImplementationName,
                    OutputDirectory = outDir
                };
                with.ProbeDirectories.Add(found.DependencyDirectory);
                DecoratorGenerationReport withProbe = command.Execute(with);

                BamDecoratorGenerationConfig withoutBase = new BamDecoratorGenerationConfig
                {
                    AssemblyPath = lostBase.ServicePath,
                    InterfaceTypeName = lostBase.InterfaceName,
                    ImplementationTypeName = lostBase.ImplementationName,
                    OutputDirectory = Path.Combine(root, "out-base")
                };
                DecoratorGenerationReport withoutBaseProbe = command.Execute(withoutBase);

                BamDecoratorGenerationConfig withBase = new BamDecoratorGenerationConfig
                {
                    AssemblyPath = foundBase.ServicePath,
                    InterfaceTypeName = foundBase.InterfaceName,
                    ImplementationTypeName = foundBase.ImplementationName,
                    OutputDirectory = Path.Combine(root, "out-base")
                };
                withBase.ProbeDirectories.Add(foundBase.DependencyDirectory);
                DecoratorGenerationReport withBaseProbe = command.Execute(withBase);
                return new ProbeOutcome(withoutProbe, withProbe, lost.DependencyName, withoutBaseProbe, withBaseProbe, lostBase.DependencyName);
            })
            .TheTest
            .ShouldPass<ProbeOutcome>((because, outcome) =>
            {
                because.ItsTrue("without the probe directory the service is skipped", outcome.WithoutProbe.Written.Count == 0 && outcome.WithoutProbe.Skipped.Count == 1, string.Join("; ", outcome.WithoutProbe.Skipped));
                because.ItsTrue("the reason names the dependency that could not be loaded", outcome.WithoutProbe.Skipped.Count == 1 && outcome.WithoutProbe.Skipped[0].Reason.Contains(outcome.LostDependencyName), outcome.WithoutProbe.Skipped.FirstOrDefault()?.Reason);
                because.ItsTrue("with the probe directory the decorator is written", FileNames(outcome.WithProbe.Written) == "WidgetDecorator.cs" && outcome.WithProbe.Skipped.Count == 0, FileNames(outcome.WithProbe.Written) + " " + string.Join("; ", outcome.WithProbe.Skipped));
                because.ItsTrue("an interface whose base is in the missing dependency is skipped", outcome.WithoutBaseProbe.Written.Count == 0 && outcome.WithoutBaseProbe.Skipped.Count == 1, string.Join("; ", outcome.WithoutBaseProbe.Skipped));
                because.ItsTrue("as a dependency that could not be loaded, named, not as a type that was not found", outcome.WithoutBaseProbe.Skipped.Count == 1 && outcome.WithoutBaseProbe.Skipped[0].Reason.Contains("a dependency could not be loaded") && outcome.WithoutBaseProbe.Skipped[0].Reason.Contains(outcome.LostBaseDependencyName) && !outcome.WithoutBaseProbe.Skipped[0].Reason.Contains("was not found"), outcome.WithoutBaseProbe.Skipped.FirstOrDefault()?.Reason);
                because.ItsTrue("with the probe directory that decorator is written too", FileNames(outcome.WithBaseProbe.Written) == "WidgetDecorator.cs" && outcome.WithBaseProbe.Skipped.Count == 0, FileNames(outcome.WithBaseProbe.Written) + " " + string.Join("; ", outcome.WithBaseProbe.Skipped));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void PreferTheDependencyBesideTheAssemblyOverAProbeDirectory()
        {
            string root = TestOutputDir("probe-order");
            string outDir = Path.Combine(root, "out");
            string name = "Beside" + Guid.NewGuid().ToString("N").Substring(0, 8);

            // The service's real dependency, then a copy of it beside the service; in the probe directory, an
            // assembly of the same name that has no Token type at all (a stale build, or a planted one).
            CompiledService service = CompileService(root, name, false);
            string besideService = Path.Combine(Path.GetDirectoryName(service.ServicePath)!, service.DependencyName + ".dll");
            File.Copy(Path.Combine(service.DependencyDirectory, service.DependencyName + ".dll"), besideService);
            string shadowDir = Path.Combine(root, name, "shadow");
            Directory.CreateDirectory(shadowDir);
            File.WriteAllBytes(Path.Combine(shadowDir, service.DependencyName + ".dll"), new RoslynCompiler().Compile(service.DependencyName,
                "namespace " + name + ".Dep { public class SomethingElse { } }"));

            After.Setup(reg =>
            {
                reg.For<GenerateDecoratorCommand>().Use(new GenerateDecoratorCommand());
            })
            .When<GenerateDecoratorCommand>("is given a probe directory holding another build of a dependency that sits beside the service", command =>
            {
                BamDecoratorGenerationConfig config = new BamDecoratorGenerationConfig
                {
                    AssemblyPath = service.ServicePath,
                    InterfaceTypeName = service.InterfaceName,
                    ImplementationTypeName = service.ImplementationName,
                    OutputDirectory = outDir
                };
                config.ProbeDirectories.Add(shadowDir);
                DecoratorGenerationReport report = command.Execute(config);
                System.Reflection.Assembly? loaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == service.DependencyName);
                return new ProbeOrderOutcome(report, loaded?.Location ?? string.Empty, besideService);
            })
            .TheTest
            .ShouldPass<ProbeOrderOutcome>((because, outcome) =>
            {
                because.ItsTrue("the decorator is written from the dependency beside the service", FileNames(outcome.Report.Written) == "WidgetDecorator.cs" && outcome.Report.Skipped.Count == 0, FileNames(outcome.Report.Written) + " " + string.Join("; ", outcome.Report.Skipped));
                because.ItsTrue("the copy beside the service is the one loaded, not the probe directory's", string.Equals(outcome.LoadedFrom, outcome.BesideService, StringComparison.OrdinalIgnoreCase), outcome.LoadedFrom);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        // Compiles {name}Dep.dll (a Token class) into <root>/<name>/dep and {name}Svc.dll (IWidget + Widget whose
        // method returns a Token) into <root>/<name>/svc, so the service's dependency is not beside it.
        // With inheritFromDependency, IWidget also derives from an IBase declared in the dependency.
        private static CompiledService CompileService(string root, string name, bool inheritFromDependency)
        {
            string dependencyName = name + "Dep";
            string serviceName = name + "Svc";
            string dependencyDir = Path.Combine(root, name, "dep");
            string serviceDir = Path.Combine(root, name, "svc");
            Directory.CreateDirectory(dependencyDir);
            Directory.CreateDirectory(serviceDir);

            RoslynCompiler compiler = new RoslynCompiler();
            string dependencyPath = Path.Combine(dependencyDir, dependencyName + ".dll");
            File.WriteAllBytes(dependencyPath, compiler.Compile(dependencyName,
                "namespace " + name + ".Dep { public interface IBase { } public class Token { public string Value { get; set; } = string.Empty; } }"));

            string servicePath = Path.Combine(serviceDir, serviceName + ".dll");
            string serviceSource =
                "namespace " + name + ".Svc {\n" +
                "  public interface IWidget" + (inheritFromDependency ? " : global::" + name + ".Dep.IBase" : string.Empty) + " { global::" + name + ".Dep.Token Make(string value); }\n" +
                "  public class Widget : IWidget { public global::" + name + ".Dep.Token Make(string value) { return new global::" + name + ".Dep.Token { Value = value }; } }\n" +
                "}\n";
            File.WriteAllBytes(servicePath, compiler.Compile(serviceName, serviceSource, () =>
                compiler.MetadataReferenceResolver.GetMetaDataReferences()
                    .Concat(new MetadataReference[] { MetadataReference.CreateFromFile(dependencyPath) })
                    .ToArray()));

            return new CompiledService(servicePath, dependencyDir, dependencyName, name + ".Svc.IWidget", name + ".Svc.Widget");
        }

        private static SkippedDecorator? OnlySkip(DecoratorGenerationReport report)
        {
            return report.Written.Count == 0 && report.Skipped.Count == 1 ? report.Skipped[0] : null;
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

        private sealed record GenerationOutcome(DecoratorGenerationReport Report, string Path, string Source);

        private sealed record TwoAssemblyOutcome(DecoratorGenerationReport WithoutImplementationPath, bool Generated, Exception? MissingAssembly);

        private sealed record MissingFieldOutcome(Exception? NoAssembly, Exception? NoInterface, DecoratorGenerationReport NoImplementation, Exception? NoOutput, Exception? NullConfig, Exception? ListedWithoutInterface, Exception? ListedWithoutAssembly);

        private sealed record UnresolvedOutcome(Exception? MissingAssembly, DecoratorGenerationReport UnknownInterface, DecoratorGenerationReport UnknownImplementation, DecoratorGenerationReport Mismatched, bool WroteRejectedPair);

        private sealed record ContinueOutcome(DecoratorGenerationReport Report, int FilesOnDisk);

        private sealed record DiscoveryOutcome(DecoratorGenerationReport Paired, DecoratorGenerationReport InterfaceOnly);

        private sealed record TemplateOutcome(string Source, Exception? Missing);

        private sealed record ProbeOutcome(DecoratorGenerationReport WithoutProbe, DecoratorGenerationReport WithProbe, string LostDependencyName, DecoratorGenerationReport WithoutBaseProbe, DecoratorGenerationReport WithBaseProbe, string LostBaseDependencyName);

        private sealed record ProbeOrderOutcome(DecoratorGenerationReport Report, string LoadedFrom, string BesideService);

        private sealed record MissingPathOutcome(Exception? LaterEntry, Exception? Discovery, Exception? ProbeDirectory, bool WroteAnything);

        private sealed record MenuOutcome(Exception? NothingWritten, Exception? Written, bool FileExists);

        private sealed record CompiledService(string ServicePath, string DependencyDirectory, string DependencyName, string InterfaceName, string ImplementationName);
    }
}
