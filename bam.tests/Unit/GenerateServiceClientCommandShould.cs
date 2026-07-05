using Bam.DependencyInjection;
using Bam.Generators;
using Bam.Test;
using Bam.Tests.TestClasses;

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
                object[] r = (object[])because.Result;
                because.ItsTrue("AssemblyPath is parsed", (string)r[0] == "./MyService.dll");
                because.ItsTrue("ServiceTypeName is parsed", (string)r[1] == "My.Ns.MyService");
                because.ItsTrue("Mode is parsed as Interface", (GenerationMode)r[2] == GenerationMode.Interface);
                because.ItsTrue("InterfaceTypeName is parsed", (string)r[3] == "My.Ns.IMyService");
                because.ItsTrue("OutputDirectory is parsed", (string)r[4] == "./out");
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
                object[] r = (object[])because.Result;
                because.ItsTrue("throws ArgumentException when AssemblyPath is missing", (bool)r[0]);
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
                object[] r = (object[])because.Result;
                because.ItsTrue("returns the EchoWebServiceClient.cs path", ((string)r[0]).EndsWith("EchoWebServiceClient.cs"));
                because.ItsTrue("wrote the client source file to disk", (bool)r[1]);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

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
                object[] r = (object[])because.Result;
                because.ItsTrue("throws ServiceClientGenerationException for non-virtual methods", (bool)r[0]);
            })
            .SoBeHappy()
            .UnlessItFailed();
        }
    }
}
