# bam.tests

Unit tests for the `bam` CLI project, verifying the command context and service registry wiring.

## Overview

The `bam.tests` project is a .NET 10 console application that uses the Bam framework's custom test runner (`bam.test`) rather than xUnit or NUnit. Tests are defined as methods with the `[UnitTest]` attribute inside classes decorated with `[UnitTestMenu]`, and they use the `When.A<T>()` fluent assertion API. The test runner is invoked via `BamConsoleContext.StaticMain`, which presents a menu-driven interface for selecting and executing tests.

The project currently contains a single test class, `BamCommandContextShould`, which validates that the `ServiceRegistry` correctly resolves and replaces `IBrokeredCommandContextResolver` implementations. The tests cover three registration strategies: generic type registration, factory delegate registration, and parameterized factory delegate registration. A `TestCommandContextResolver` stub is used as a replacement resolver during testing.

## Key Classes

| Class | Description |
|-------|-------------|
| `Program` | Entry point. Calls `BamConsoleContext.StaticMain(args)` to launch the menu-driven test runner. |
| `BamCommandContextShould` | Unit test class containing three tests that verify `ServiceRegistry` can swap `IBrokeredCommandContextResolver` implementations using different registration strategies (generic type, factory, parameterized factory). |
| `TestCommandContextResolver` | A test double for `CommandContextResolver`. Accepts an `IBamContext` in its constructor; `LoadContexts()` throws `NotImplementedException` (intentionally -- it is only used to verify type resolution, not command loading). |

## Dependencies

### Project References
- `bam` -- the CLI project under test
- `bam.base` -- core framework primitives
- `bam.command` -- command infrastructure being tested
- `bam.console` -- provides `BamConsoleContext.StaticMain` for the test runner entry point
- `bam.data.repositories` -- data repository abstractions
- `bam.data.schema` -- data schema support
- `bam.data` -- data layer
- `bam.test` -- the custom test framework (`UnitTestMenu`, `UnitTest`, `When.A<T>()`, etc.)

### Package References
None.

## Usage Examples

### Running the tests
```bash
dotnet run --project bam.tests/bam.tests.csproj -- --ut
```
**Important:** Use `--ut` (not `/ut`) when running from Git Bash, because Git Bash rewrites `/ut` to a filesystem path.

### Writing a test in the Bam test style
```csharp
using Bam.DependencyInjection;
using Bam.Test;

[UnitTestMenu("MyComponent should")]
public class MyComponentShould : UnitTestMenuContainer
{
    public MyComponentShould(ServiceRegistry serviceRegistry) : base(serviceRegistry) { }

    [UnitTest]
    public void DoSomethingCorrectly()
    {
        When.A<MyComponent>("does something",
            new MyComponent(),
            (component) =>
            {
                return component.DoSomething();
            })
        .TheTest
        .ShouldPass(because =>
        {
            because.ItsTrue("result is correct", (bool)because.Result);
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
```

## Known Gaps / Not Yet Implemented

- **`TestCommandContextResolver.LoadContexts()`** -- Throws `NotImplementedException` intentionally. This is a test stub; `LoadContexts` is not called in any current test scenario.
- **Limited test coverage** -- Only `BamCommandContextShould` exists. There are no tests for `CompositeCommandBroker`, `CompositeCommandRunner`, `ConsoleCommands`, or `ToolsProcessCommandContextResolver`.
