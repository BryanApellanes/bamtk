# bam

Bam Framework CLI Tool -- the main command-line entry point for the Bam toolkit.

## Overview

The `bam` project is a .NET 10 console application that serves as the primary command-line interface for the Bam framework. It bootstraps a service registry, wires up command parsing and brokering infrastructure, and delegates execution to `BamCommandContext.Main`. The CLI supports both interactive menu-driven commands (via `ConsoleMenuContainer`) and process-based external tool invocation.

The application uses a composite command resolution strategy: it merges menu commands (defined via `[ConsoleCommand]` attributes) with process commands (executables discovered on disk, including a `.bam/tools` directory). This allows users to extend the CLI by dropping executables into the tools folder without modifying source code.

The project is packaged as a NuGet tool (`bam` package, version 2.0.0) and publishes into `~/.bam/build/pack/`.

## Key Classes

| Class | Description |
|-------|-------------|
| `Program` | Entry point. Configures the `ServiceRegistry` with command infrastructure bindings and calls `BamCommandContext.Main`. |
| `CompositeCommandBroker` | A `CommandBroker` that resolves commands from multiple sources (menus and processes) via `CompositeCommandContextResolver`. |
| `CompositeCommandContextResolver` | Aggregates `MenuCommandContextResolver` and `ProcessCommandContextResolver` to load command contexts from both interactive menus and external executables. |
| `CompositeCommandRunner` | Intended to run brokered commands from composite sources. **Not yet implemented.** |
| `ConsoleCommands` | A sample `ConsoleMenuContainer` registered under the `"code"` menu, demonstrating `[ConsoleCommand]` usage with string parameters and a default command. |
| `ToolsProcessCommandContextResolver` | Extends `ProcessCommandContextResolver` to search `.bam/tools` alongside default directories for executable commands. |
| `GenerateMenuContainer` | The `generate` menu: `service-client` and `decorator` commands, each with an `-init` companion that writes a starter config. |
| `GenerateServiceClientCommand` / `GenerateDecoratorCommand` | The console-independent actions behind the `generate` commands. Each validates its config, loads the target assembly and drives the matching `bam.generators` generator. |

## Dependencies

### Project References
- `bam.base` -- core framework primitives (`BamProfile`, `ServiceRegistry`, etc.)
- `bam.command` -- command brokering, parsing, and execution infrastructure (`BamCommandContext`, `CommandBroker`, `ProcessCommandContextResolver`, etc.)
- `bam.generators.client` -- the service-client generator behind `bam generate service-client`
- `bam.generators.decorators` -- the decorator generator behind `bam generate decorator`

### Package References
None (relies solely on project references).

## Usage Examples

### Running the CLI
```bash
dotnet run --project bam/bam.csproj -- <command> [arguments]
```

### Generating code
The `generate` menu holds the code-generation commands. Each reads its input from a `--config` YAML or JSON file, and each has an `-init` companion that writes a starter config to the current directory.

```bash
bam generate service-client-init    # writes ./BamServiceClientGenerationConfig.yaml
bam generate ServiceClient --config /full/path/to/BamServiceClientGenerationConfig.yaml

bam generate decorator-init         # writes ./BamDecoratorGenerationConfig.yaml
bam generate Decorator --config /full/path/to/BamDecoratorGenerationConfig.yaml
```

Pass `--config` a full path. A relative one is resolved against `~/.bam/data/arguments`, not the current directory. Paths *inside* the config (`AssemblyPath`, `ImplementationAssemblyPath`, `OutputDirectory`) are different: they resolve against the current directory, not the config file's location. Run the command from the directory the starter's `./` paths assume, or make them absolute. Re-running an `-init` command overwrites the config it wrote.

`Decorator` writes `{ImplementationName}Decorator.cs`: a typed decorator for a service interface plus the extension methods for subscribing handlers to it. See `submodules/bam.generators/bam.generators.decorators/README.md`.

### Defining a new console command
```csharp
using Bam.Console;
using Bam.DependencyInjection;
using Bam.Services;

[ConsoleMenu("mycommands")]
public class MyCommands : ConsoleMenuContainer
{
    public MyCommands(ServiceRegistry serviceRegistry) : base(serviceRegistry) { }

    public override ServiceRegistry Configure(ServiceRegistry serviceRegistry) => serviceRegistry;

    [ConsoleCommand("greet")]
    public void Greet(string name)
    {
        Message.PrintLine("Hello, {0}!", name);
    }
}
```

### Registering a custom command broker
```csharp
ServiceRegistry registry = BamCommandContext.Current.ServiceRegistry;
registry
    .For<ICommandBroker>().Use<CompositeCommandBroker>();
```

## Known Gaps / Not Yet Implemented

- **`CompositeCommandRunner.Run`** -- The `Run` method throws `NotImplementedException`. It is declared but has no functional implementation, meaning composite command execution through this runner is not yet operational.
- **`ConsoleCommands`** -- Contains only demonstration/test commands (`testWithStringParameters`, `SomeRandomName`, `another`, `DEFAULT`). No production commands are defined in this menu container.
