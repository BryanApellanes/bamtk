using Bam.DependencyInjection;
using Bam.Generators;
using Bam.Server;
using Bam.Test;
using Bam.Tests.TestClasses;
using Microsoft.CodeAnalysis;

namespace Bam.Tests.Unit
{
    [UnitTestMenu("GenerateServiceClientCommand should")]
    public class GenerateServiceClientCommandShould : UnitTestMenuContainer
    {
        public GenerateServiceClientCommandShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }

        private static string TestOutputDir(string name) =>
            Path.Combine(Path.GetTempPath(), "bam-service-client-tests", name);

        [UnitTest(RunSynchronously = true)]
        public void ReadConfigFromYaml()
        {
            string dir = TestOutputDir("roundtrip");
            Directory.CreateDirectory(dir);
            string yamlPath = Path.Combine(dir, "config.yaml");
            File.WriteAllText(yamlPath,
                "AssemblyPath: ./MyService.dll\n" +
                "ServiceTypeName: My.Ns.MyService\n" +
                "Mode: Interface\n" +
                "InterfaceTypeName: My.Ns.IMyService\n" +
                "OutputDirectory: ./out\n");

            When.A<string>("reads a service-client config from YAML", yamlPath,
                path =>
                {
                    BamServiceClientGenerationConfig cfg = BamServiceClientGenerationConfig.ReadFrom(path);
                    return new object[]
                    {
                        cfg.AssemblyPath, cfg.ServiceTypeName, cfg.Mode, cfg.InterfaceTypeName ?? string.Empty, cfg.OutputDirectory
                    };
                })
            .TheTest
            .ShouldPass(because =>
            {
                because.TheResult
                    .As<object[]>("AssemblyPath is parsed", r => (string)r[0] == "./MyService.dll")
                    .As<object[]>("ServiceTypeName is parsed", r => (string)r[1] == "My.Ns.MyService")
                    .As<object[]>("Mode is parsed as Interface", r => (GenerationMode)r[2] == GenerationMode.Interface)
                    .As<object[]>("InterfaceTypeName is parsed", r => (string)r[3] == "My.Ns.IMyService")
                    .As<object[]>("OutputDirectory is parsed", r => (string)r[4] == "./out");
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void RejectConfigWithMissingRequiredFields()
        {
            BamServiceClientGenerationConfig config = new BamServiceClientGenerationConfig
            {
                AssemblyPath = string.Empty,
                ServiceTypeName = "X",
                OutputDirectory = "./out"
            };

            When.A<BamServiceClientGenerationConfig>("rejects a config with no AssemblyPath", config,
                cfg =>
                {
                    try
                    {
                        new GenerateServiceClientCommand().Execute(cfg);
                        return new object[] { false };
                    }
                    catch (ArgumentException)
                    {
                        return new object[] { true };
                    }
                })
            .TheTest
            .ShouldPass(because =>
            {
                because.TheResult
                    .As<object[]>("throws ArgumentException when AssemblyPath is missing", r => (bool)r[0]);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void GenerateSubclassClientToDisk()
        {
            string outDir = TestOutputDir("subclass");
            if (Directory.Exists(outDir))
            {
                Directory.Delete(outDir, true);
            }

            BamServiceClientGenerationConfig config = new BamServiceClientGenerationConfig
            {
                AssemblyPath = typeof(EchoWebService).Assembly.Location,
                ServiceTypeName = typeof(EchoWebService).FullName!,
                Mode = GenerationMode.Subclass,
                OutputDirectory = outDir
            };

            When.A<BamServiceClientGenerationConfig>("generates a subclass client file", config,
                cfg =>
                {
                    string path = new GenerateServiceClientCommand().Execute(cfg);
                    return new object[] { path, File.Exists(path) };
                })
            .TheTest
            .ShouldPass(because =>
            {
                because.TheResult
                    .As<object[]>("returns the EchoWebServiceClient.cs path", r => ((string)r[0]).EndsWith("EchoWebServiceClient.cs"))
                    .As<object[]>("wrote the client source file to disk", r => (bool)r[1]);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void ResolveADependencyBesideACompiledTarget()
        {
            // bamtk#111 moved this command off Assembly.LoadFrom onto AssemblyProbe. A target that is not already
            // loaded, whose public surface uses a type from a dependency in the same directory, has to go through
            // the probe's resolution to be generated at all.
            string tag = Guid.NewGuid().ToString("N").Substring(0, 8);
            string root = TestOutputDir("beside-" + tag);
            CompiledWebService service = CompileWebService(root, "Beside" + tag, null);

            When.A<GenerateServiceClientCommand>("generates a client for a compiled target with its dependency beside it", new GenerateServiceClientCommand(), command =>
            {
                string path = command.Execute(new BamServiceClientGenerationConfig
                {
                    AssemblyPath = service.ServicePath,
                    ServiceTypeName = service.ServiceTypeName,
                    Mode = GenerationMode.Subclass,
                    OutputDirectory = Path.Combine(root, "out")
                });
                return new ClientOutcome(path, File.Exists(path) ? File.ReadAllText(path) : string.Empty, null, Array.Empty<string>());
            })
            .TheTest
            .ShouldPass<ClientOutcome>((because, outcome) =>
            {
                because.ItsTrue("the client is written", outcome.Source.Length > 0, outcome.Path);
                because.ItsTrue("and uses the dependency's type", outcome.Source.Contains("Token"), outcome.Source.Length > 400 ? outcome.Source.Substring(0, 400) : outcome.Source);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest(RunSynchronously = true)]
        public void NotFollowAReferenceNameOutOfTheTargetsDirectory()
        {
            // The same target with its dependency's reference renamed to a path that leaves the directory, and a
            // matching file waiting there: the command must fail without loading it.
            string tag = Guid.NewGuid().ToString("N").Substring(0, 8);
            string root = TestOutputDir("escaping-" + tag);
            string outside = Path.Combine(root, "outside");
            CompiledWebService service = CompileWebService(root, "Escape" + tag, "../outside/Out" + tag);

            When.A<GenerateServiceClientCommand>("generates a client for a target whose dependency's reference name is a path", new GenerateServiceClientCommand(), command =>
            {
                string outputDirectory = Path.Combine(root, "out");
                Exception? thrown = null;
                try
                {
                    command.Execute(new BamServiceClientGenerationConfig
                    {
                        AssemblyPath = service.ServicePath,
                        ServiceTypeName = service.ServiceTypeName,
                        Mode = GenerationMode.Subclass,
                        OutputDirectory = outputDirectory
                    });
                }
                catch (Exception ex)
                {
                    thrown = ex;
                }

                string[] loadedFromOutside = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly => !assembly.IsDynamic && assembly.Location.StartsWith(outside, StringComparison.OrdinalIgnoreCase))
                    .Select(assembly => assembly.Location)
                    .ToArray();
                string[] written = Directory.Exists(outputDirectory) ? Directory.GetFiles(outputDirectory) : Array.Empty<string>();
                return new ClientOutcome(string.Join(", ", written), string.Empty, thrown, loadedFromOutside);
            })
            .TheTest
            .ShouldPass<ClientOutcome>((because, outcome) =>
            {
                because.ItsTrue("nothing outside the target's directory was loaded", outcome.LoadedFromOutside.Length == 0, string.Join(", ", outcome.LoadedFromOutside));
                because.ItsTrue("the command fails", outcome.Thrown is not null);
                because.ItsTrue("and writes no client", outcome.Path.Length == 0, outcome.Path);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        // Compiles {name}Dep.dll (a Token class) and {name}Svc.dll (a [WebService] whose virtual method returns a
        // Token) into <root>/svc. With escapeTo, the service's reference to the dependency is renamed to that path
        // and a dependency named after its last segment is written under <root>/outside, where the path leads.
        private static CompiledWebService CompileWebService(string root, string name, string? escapeTo)
        {
            string serviceDir = Path.Combine(root, "svc");
            string buildDir = Path.Combine(root, "build");
            Directory.CreateDirectory(serviceDir);
            Directory.CreateDirectory(buildDir);
            string dependencyNamespace = name + "Dep";
            string dependencySource = "namespace " + dependencyNamespace + " { public class Token { public string Value { get; set; } = string.Empty; } }";
            string dependencyName = escapeTo is null ? name + "Dep" : ReferenceNamePatcher.PlaceholderFor(name, escapeTo);
            RoslynCompiler compiler = new RoslynCompiler();
            string dependencyPath = Path.Combine(escapeTo is null ? serviceDir : buildDir, dependencyName + ".dll");
            File.WriteAllBytes(dependencyPath, compiler.Compile(dependencyName, dependencySource));

            string serviceName = name + "Svc";
            byte[] image = compiler.Compile(serviceName,
                "namespace " + serviceName + " {\n" +
                "  [global::Bam.Server.WebService]\n" +
                "  public class TokenService { public virtual global::" + dependencyNamespace + ".Token Make(string value) { return new global::" + dependencyNamespace + ".Token { Value = value }; } }\n" +
                "}\n",
                () => compiler.MetadataReferenceResolver.GetMetaDataReferences()
                    .Concat(new MetadataReference[] { MetadataReference.CreateFromFile(dependencyPath), MetadataReference.CreateFromFile(typeof(WebServiceAttribute).Assembly.Location) })
                    .ToArray());
            if (escapeTo is not null)
            {
                ReferenceNamePatcher.Rename(image, dependencyName, escapeTo);
                string leaf = escapeTo.Substring(escapeTo.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
                string outside = Path.GetFullPath(Path.Combine(serviceDir, escapeTo.Substring(0, escapeTo.Length - leaf.Length)));
                Directory.CreateDirectory(outside);
                File.WriteAllBytes(Path.Combine(outside, leaf + ".dll"), compiler.Compile(leaf, dependencySource));
            }

            string servicePath = Path.Combine(serviceDir, serviceName + ".dll");
            File.WriteAllBytes(servicePath, image);
            return new CompiledWebService(servicePath, serviceName + ".TokenService");
        }

        private sealed record CompiledWebService(string ServicePath, string ServiceTypeName);

        private sealed record ClientOutcome(string Path, string Source, Exception? Thrown, string[] LoadedFromOutside);

        [UnitTest(RunSynchronously = true)]
        public void RejectNonVirtualServiceInSubclassMode()
        {
            BamServiceClientGenerationConfig config = new BamServiceClientGenerationConfig
            {
                AssemblyPath = typeof(NonVirtualWebService).Assembly.Location,
                ServiceTypeName = typeof(NonVirtualWebService).FullName!,
                Mode = GenerationMode.Subclass,
                OutputDirectory = TestOutputDir("nonvirtual")
            };

            When.A<BamServiceClientGenerationConfig>("rejects a non-virtual service in subclass mode", config,
                cfg =>
                {
                    try
                    {
                        new GenerateServiceClientCommand().Execute(cfg);
                        return new object[] { false };
                    }
                    catch (ServiceClientGenerationException)
                    {
                        return new object[] { true };
                    }
                })
            .TheTest
            .ShouldPass(because =>
            {
                because.TheResult
                    .As<object[]>("throws ServiceClientGenerationException for non-virtual methods", r => (bool)r[0]);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }
    }
}
