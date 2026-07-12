# CLAUDE.md

## Project Overview

**bamtk** (the Bam Toolkit) is a comprehensive, entirely custom **C# .NET framework, SDK, and CLI** for rapidly building fully distributed full-stack applications — spanning data modeling/storage, view management/rendering, and business-logic processing. It aspires first and foremost to be a **CLI toolkit**: the `bam` command is the primary entry point, and the framework is designed so you can build end-to-end user-facing systems, web services, CLIs, or any combination.

It is a self-contained stack with **no reliance on mainstream third-party frameworks** for its core concerns: its own dependency injection (`ServiceRegistry`), its own test framework (`bamtest` / `bam.test`), and its own multi-database ORM. There is no xUnit/NUnit/MSTest.

- **Solution:** `bamtk.sln`
- **Primary branch:** `bamtk` (not `master`/`main`)
- **44 framework libraries** registered as git submodules under `submodules/`
- **CLI tool:** `bam/` — a .NET 10 console app packaged as the `bam` NuGet tool (v2.0.0)
- **Author / Company:** Bryan Apellanes / Three Headz

`bamtk` is the standalone development checkout of the framework. The `Server1` monorepo (`c:/src/repos/Server1`) embeds it as a **nested** git submodule at `domains/*/.bam/submodules/bamtk/`. Framework development happens here; Server1's nested submodules are then advanced to the latest commit.

## Repository Structure

```
bamtk/
├── bam/                     # The `bam` CLI console app (.NET 10 NuGet tool)
├── bam.tests/               # Tests for the CLI
├── submodules/              # 44 framework libraries, each its own git submodule
│   ├── bam.base/            #   core primitives: ServiceRegistry, BamProfile, ...
│   ├── bam.command/         #   command brokering/parsing (BamCommandContext, CommandBroker)
│   ├── bam.console/         #   ConsoleMenuContainer, [ConsoleCommand]
│   ├── bam.application/     #   application hosting
│   ├── bam.configuration/   #   configuration
│   ├── bam.logging*/        #   logging + counters
│   ├── bam.caching/         #   caching
│   ├── bam.encryption/      #   encryption
│   ├── bam.storage/ bamfs/  #   blob/file storage
│   ├── bam.data*/           #   ORM: repositories, schema, dynamic queries, objects, graph (GraphQL)
│   ├── bamdb/               #   RDBMS + REST integrations (see rdbms/ and rest/ READMEs)
│   ├── bam.server/ bamserver/ bam.protocol/ bamsvc/ bamapi/ bam.openapi/   # server + API stack
│   ├── bamux/               #   UX server
│   ├── bam.maui/ bam.presentation/   # UI (MAUI, presentation)
│   ├── bamtest/ bam.test/   #   custom test framework + Cobertura coverage
│   ├── bam.generators/      #   Handlebars-based code generation
│   ├── bam.shell/ bam.automation/ bam.remote/ bambot/ bake/ bamd/   # tooling/automation
│   └── legacy/              #   legacy code (bam.net.shared) — avoid modifying
├── bamtk.sln                # Aggregates all submodule projects (~230 projects)
├── run-tests.sh             # Build + run bamtest with Cobertura → HTML coverage
├── commit-submodules.sh     # Commit across submodules
├── git-rm-submodule.sh      # Remove a submodule cleanly
└── replace_branch.sh        # Submodule branch helper
```

> **Untracked / WIP:** `submodules/bamvoice/` exists on disk but is **not** registered in `.gitmodules`. Treat git as authoritative for what belongs to the toolkit.

## Build & Run Commands

```bash
# Build the whole toolkit
dotnet build bamtk.sln

# Build / run the CLI
dotnet build bam/bam.csproj
dotnet run --project bam/bam.csproj -- <command> [arguments]

# Build a single framework library
dotnet build submodules/bam.base/bam.base/bam.base.csproj
```

- **Build output** is redirected out of the tree to `~/.bam/build/{Configuration}/{project}/` (see the `OutputPath` in each csproj), and NuGet packs to `~/.bam/build/pack/`. The `bam` tool publishes to `~/.bam/build/pack/`.

## Testing

- **Framework:** custom `bamtest` / `bam.test` (never xUnit/NUnit/MSTest, no external mocking framework assumptions).
- **Convention:** test projects use `*.tests` naming (e.g. `bam.command.tests`, `bam.protocol.tests`), plus the root `bam.tests` for the CLI.
- **Run all tests with coverage:**
  ```bash
  ./run-tests.sh            # defaults to the unit-test switch (--ut / /ut)
  ```
  The script builds `bamtest`, builds the solution into a per-SHA run dir, runs tests, emits Cobertura XML, and generates an HTML report via `reportgenerator`. Artifacts are rooted at `~/.bam/test/runs/{git-short-sha}/` (`coverage-report/index.html`).
- **Argument style:** switches use Windows style (`/switch:value`) on Windows and POSIX style (`--switch=value`) elsewhere; override with `BAM_ARG_STYLE=Windows|Posix`. `run-tests.sh` sets `MSYS_NO_PATHCONV=1` to stop Git Bash mangling `/switch` paths.
- **Baseline first:** establish a passing/failing baseline from the `bamtk` branch before changing code; some tests may be flaky and fail for reasons unrelated to a given change.

## Coding Style

### Project Settings

- **Target framework:** `net10.0` for the bulk of projects (a few, e.g. `bake`, still target `net8.0`).
- **Nullable reference types:** enabled
- **Implicit usings:** enabled
- **RootNamespace:** `Bam` (regardless of the `bam.*` assembly/package id)
- **Packaging:** `GeneratePackageOnBuild` is on; each library ships as a `bam.*` NuGet package (Company: Three Headz).

### Naming Conventions

| Element           | Convention        | Example                                 |
|-------------------|-------------------|-----------------------------------------|
| Methods           | PascalCase        | `IncrementCount()`, `Subscribe()`       |
| Properties        | PascalCase        | `public string Uuid { get; set; }`      |
| Local variables   | camelCase         | `int currentCount = 0;`                 |
| Parameters        | camelCase         | `(string paramName = "param")`          |
| Private fields    | `_camelCase`      | `ServiceRegistry _serviceRegistry;`     |
| Interfaces        | `I{Name}`         | `ICommandBroker`, `ILogger`             |
| Interface files   | `I{Name}.cs`      | `ICommandBroker.cs`                      |
| Exception classes | `{Name}Exception` | `DependencyLoopException`               |
| Attribute classes | `{Name}Attribute` | `ConsoleCommandAttribute`               |

### Type Usage Conventions

- **Never use `var`.** Always declare the explicit/concrete type of a local variable (`RegistrationService registrationService = new(...)`, not `var registrationService = new(...)`). **Exception: generated code** — files with an `// <auto-generated>` header or produced by `bam.generators`/DAO codegen may use `var` freely; do not hand-edit generated files anyway.
- **Never use anonymous types/objects** (`new { Foo = 1 }`) in hand-written code. If a concrete type doesn't exist yet, define one — a plain class with named properties, even if minimal — rather than reaching for an anonymous type. The two exceptions: (1) generated code, and (2) a deliberately-flagged, temporary quick fix meant to be revisited (mark it with a `// TODO:` naming the concrete type to introduce).
- **Never return `object`/`object?` from a method** unless there is an explicit, justified need (e.g. a truly polymorphic API boundary). A method whose result varies in shape should return a concrete, named type — introduce one rather than defaulting to `object`.
- Note: existing checked-in code predates these rules in places (e.g. `var` usage in older files) — these conventions apply to new/changed code going forward, not a mandate to retroactively rewrite untouched code.

### Namespace Conventions

- `Bam` — root namespace for all framework code (note the mismatch with lowercase `bam.*` package ids).
- `Bam.{SubSystem}` — e.g. `Bam.DependencyInjection`, `Bam.Logging`, `Bam.Data.Repositories`, `Bam.Console`, `Bam.Services`.

### Dependency Injection

Custom `ServiceRegistry` with a fluent API — not Microsoft.Extensions.DependencyInjection:

```csharp
serviceRegistry
    .For<ICommandBroker>()
    .Use<CompositeCommandBroker>();
```

The `bam` CLI bootstraps a `ServiceRegistry`, wires command infrastructure, and delegates to `BamCommandContext.Main`.

### CLI Commands

Two command sources are merged by `CompositeCommandContextResolver`:

1. **Menu commands** — methods marked `[ConsoleCommand]` on a `ConsoleMenuContainer`:
   ```csharp
   [ConsoleMenu("mycommands")]
   public class MyCommands : ConsoleMenuContainer
   {
       public MyCommands(ServiceRegistry serviceRegistry) : base(serviceRegistry) { }
       public override ServiceRegistry Configure(ServiceRegistry serviceRegistry) => serviceRegistry;

       [ConsoleCommand("greet")]
       public void Greet(string name) => Message.PrintLine("Hello, {0}!", name);
   }
   ```
2. **Process commands** — executables discovered on disk, including a `.bam/tools` directory (`ToolsProcessCommandContextResolver`). Drop an executable into `.bam/tools` to extend the CLI without changing source.

> Known gap: `CompositeCommandRunner.Run` throws `NotImplementedException` — composite execution through that runner is not yet operational.

### Code Generation

Handlebars templates live under `bam.generators/` (e.g. `Class.hbs`, `Dto.hbs`, `Context.hbs`). Generated files carry a generated-header comment — do not edit them by hand; regenerate instead.

## Git Workflow

- **Main branch:** `bamtk` (not `master`/`main`).
- **Submodules:** 44 libraries, all on the `git@github.com:BryanApellanes/<name>.git` remote pattern. After changing a submodule, commit inside it first, then update the pointer in `bamtk`.
- **Helpers:** `commit-submodules.sh` (commit across submodules), `git-rm-submodule.sh` (remove a submodule), `replace_branch.sh`.
- **Worktrees:** per global convention, start code work in a worktree at `~/.claude/workspaces/{branchName}` based on `bamtk`.

## Entry-Point READMEs

- `bam/README.md` — the CLI (composite command resolution, `[ConsoleCommand]`, `.bam/tools`).
- `submodules/bamdb/rdbms/README.md` — RDBMS integrations (MSSQL, PostgreSQL, MySQL, Oracle, Firebird).
- `submodules/bamdb/rest/README.md` — REST server over the data layer.
- `submodules/bamux/README.md` — UX server.
- `submodules/bamsvc/README.md` — API server.

## Important Notes

- **`submodules/legacy/`** holds legacy code — avoid modifying unless explicitly asked.
- **Custom stack everywhere:** reach for `ServiceRegistry`, `bamtest`, and the built-in data/server libraries before introducing an external dependency; matching the existing pattern is the priority.
- **Build artifacts live under `~/.bam/`**, not `bin/`/`obj/` in-tree — check there when a build "produced nothing."
- When making design decisions or bug fixes, reference the entire codebase (all 44 submodules) and the relevant entry-point READMEs for context.
