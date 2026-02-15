# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the entire solution
dotnet build pondhawk-tools.slnx

# Build a specific project
dotnet build src/Pondhawk.Core/Pondhawk.Core.csproj
dotnet build src/Pondhawk.Watch/Pondhawk.Watch.csproj
dotnet build src/Pondhawk.Rules/Pondhawk.Rules.csproj
dotnet build src/Pondhawk.Rql/Pondhawk.Rql.csproj
dotnet build src/Pondhawk.Hosting/Pondhawk.Hosting.csproj
dotnet build src/Pondhawk.Rules.EFCore/Pondhawk.Rules.EFCore.csproj
```

## Project Setup

- **.NET 10** targeting `net10.0` (SDK 10.0.103)
- **Central package management** via `Directory.Packages.props`
- **Nullable reference types** enabled in Core and Watch projects
## Architecture

This repository contains class libraries under `src/` that form the **Pondhawk** toolkit by Pond Hawk Technologies:

### Pondhawk.Core — Shared Foundation

Core utilities, pipeline infrastructure, and common exception types. Key subsystems:

- **Utilities**: Pipeline infrastructure (`IServiceCollection`-based DI registration), process utilities, type extensions.
- **Exceptions**: Common exception types.

### Pondhawk.Watch — Serilog Sink + Logging API (standalone, multi-targeted)

A Serilog `ILogEventSink` with Channel-based batching, plus the full Watch logging API. Fully standalone — no dependency on Pondhawk.Core. Multi-targeted: `net10.0` and `netstandard2.0`.

- **Logging API** (`Pondhawk.Watch` namespace): `SerilogExtensions` provides extensions on both `Serilog.ILogger` and `object`:
  - **`object.GetLogger()`** — returns a `Serilog.ILogger` with `SourceContext` set to the concise full name of the object's type
  - **`object.EnterMethod()`** / **`ILogger.EnterMethod()`** — disposable method tracing scope with automatic entry/exit logging and elapsed time (.NET 7+ only)
  - **`ILogger.Inspect(name, value)`** — logs a name/value pair as `"{Name} = {Value}"` at Debug level
  - **`ILogger.LogObject(value)`** — serializes an object to JSON payload
  - **`ILogger.LogJson/LogSql/LogXml/LogYaml/LogText(title, content)`** — typed payload logging with syntax highlighting hints
  - Also: `WatchSwitchConfig`, `WatchPropertyNames`, serializers (`JsonObjectSerializer`), `PayloadType` enum, `[Sensitive]` attribute, `CorrelationManager`.
- **WatchSink**: `ILogEventSink` implementation with unbounded Channel batching. Converts Serilog events to Watch `LogEvent` instances with switch-based filtering. Circuit breaker for HTTP resilience.
- **WatchSinkExtensions**: Serilog `LoggerConfiguration` extension method for configuring the Watch sink.
- **Switching**: Dynamic log level control via `SwitchSource`/`SwitchDef` with pattern matching. `WatchSwitchSource` polls a Watch Server for switch configuration.
- **LogEvent/LogEventBatch**: Event model with dual serialization: MemoryPack+Brotli on .NET 7+ (`#if NET7_0_OR_GREATER`), System.Text.Json on netstandard2.0.
- **netstandard2.0 note**: `MethodLogger` and `EnterMethod()` require .NET 7+ (Serilog ILogger default interface methods). All other logging APIs (GetLogger, LogObject, LogJson, etc.) work on all targets.

### Pondhawk.Rules — Rule Engine (standalone, no Core dependency)

A forward-chaining rule engine with type-based fact matching. Fully standalone — no dependency on Pondhawk.Core. Uses `Microsoft.Extensions.Logging` for listener infrastructure (Serilog picks up MS Logging events transparently). Key subsystems:

- **Builder** (`Pondhawk.Rules.Builder`): Fluent API for defining rules. `RuleBuilder<TFact1..TFact4>` creates `Rule<T>` instances via `When().And().Then()` chains. Supports up to 4 fact types per rule. Multi-fact rules have per-fact `When(Func<T, bool>)`/`And(Func<T, bool>)` overloads that enable C# pattern matching with full type inference. Rules have salience (priority), mutex (mutual exclusion), fire-once, inception/expiration.
- **Evaluation** (`Pondhawk.Rules.Evaluation`): `EvaluationPlan` generates all fact-type combinations using variations-with-repetition. `TupleEvaluator` executes rules in salience order against fact tuples. `FactSpace` stores facts with int selectors for memory efficiency.
- **Tree** (`Pondhawk.Rules.Tree`): `RuleTree` indexes rules by fact types for fast lookup with polymorphic type matching.
- **Validation** (`Pondhawk.Rules.Validators`): `ValidationBuilder<TFact>` with `Assert<T>(expr).Is().IsNot().Otherwise()` chains. Runs at very high salience.
- **Factory** (`Pondhawk.Rules.Factory`): `RuleSet` for runtime rule creation without predefined builder classes. `RuleSetFactory` uses `Lazy<T>` for thread-safe exactly-once initialization.
- **Listeners** (`Pondhawk.Rules.Listeners`): Observer pattern (`IEvaluationListener`) for tracing rule evaluation. `WatchEvaluationListener` uses `Microsoft.Extensions.Logging.ILogger`. `WatchEvaluationListenerFactory` uses `ILoggerFactory` (defaults to `NullLoggerFactory`).
- **RuleEvent**: Sealed event type with `IEquatable<RuleEvent>`, init-only setters, nested `EventCategory` enum (`Info`, `Warning`, `Violation`). Replaces the former Core `EventDetail` dependency.
- **Exceptions**: `ViolationsExistException`, `NoRulesEvaluatedException`, `EvaluationExhaustedException` — all extend `Exception` directly.

Evaluation flow: `RuleBuilder` → `RuleTree` (indexed by type) → `EvaluationPlan` (generates steps) → `TupleEvaluator` (executes) → `EvaluationResults` (aggregates scores/events/violations). Forward chaining via `InsertFact`/`ModifyFact`/`RetractFact` triggers re-evaluation.

**Authoring guidelines**: Avoid `||` in conditions — each OR branch should be a separate rule for atomicity and traceability. Prefer `.And()` chains over `&&` inside a single predicate.

### Pondhawk.Rules.EFCore — EF Core SaveChanges Validation

Pre-save entity validation interceptor that hooks into EF Core's `SaveChangesInterceptor`. Validates all `Added` and `Modified` entities through `Pondhawk.Rules` before they reach the database.

- **RuleValidationInterceptor**: `SaveChangesInterceptor` subclass that pulls entities from `ChangeTracker`, calls `IRuleSet.Validate()`, and throws `EntityValidationException` if validation fails. Overrides both `SavingChanges` and `SavingChangesAsync`.
- **EntityValidationException**: Carries `ValidationResult` with structured violations. Formats a human-readable message from violations.
- **DbContextOptionsBuilderExtensions**: `AddRuleValidation(IRuleSet)` convenience method.
- Minimum EF Core version: 5.0.0.

### Pondhawk.Rql — Resource Query Language (standalone, no Core dependency)

A filtering DSL with AST, fluent builder, parser, and multiple serialization targets. Fully standalone — no dependency on Pondhawk.Core.

- **AST**: `RqlTree` (root) contains `Criteria` (list of `IRqlPredicate`). `RqlOperator` enum: Equals, NotEquals, LesserThan, GreaterThan, Between, In, NotIn, StartsWith, Contains, etc.
- **Builder** (`Pondhawk.Rql.Builder`): `RqlFilterBuilder<TTarget>` provides fluent API: `.Where(expr).Equals(value).And(expr).GreaterThan(value)`. `Introspect()` builds filters from objects decorated with `[CriterionAttribute]`.
- **Parser** (`Pondhawk.Rql.Parser`): Parses RQL criteria text back into `RqlTree` AST using the **Sprache** parser combinator library. `RqlLanguageParser.ToCriteria(string)` parses criteria. Value type prefixes: `@` for DateTime, `#` for decimal, `'...'` for strings.
- **Serialization** (`Pondhawk.Rql.Serialization`): Three output formats:
  - `ToRql()` — RQL text: `(eq(Name,'John'),gt(Age,30))`
  - `ToLambda<T>()` / `ToExpression<T>()` — compiled LINQ expressions
  - `ToSqlQuery()` / `ToSqlWhere()` — parameterized SQL
  - `ToDescription()` — human-readable English: `"Name equals 'John' and Age is greater than 30"`

### Pondhawk.Hosting — Service Startup Extensions for Generic Host

Lightweight service lifecycle management for `Microsoft.Extensions.Hosting`. Standalone — no dependency on any other Pondhawk project. Only depends on `Microsoft.Extensions.Hosting.Abstractions`.

- **`AddSingletonWithStart<TService>(startAction)`** — registers a singleton and a start lambda, co-located at the registration site. Supports sync, async, and optional stop lambdas:
  ```csharp
  services.AddSingletonWithStart<RuleSetFactory>(f => f.Start());
  services.AddSingletonWithStart<RuleSetFactory>(f => f.Start(), f => f.Stop());
  services.AddSingletonWithStart<MyService>((svc, ct) => svc.InitAsync(ct), (svc, ct) => svc.StopAsync(ct));
  ```
- **ServiceStarterHostedService**: `IHostedService` that resolves all registered descriptors and calls start lambdas on host startup, stop lambdas in reverse order on shutdown. Auto-registered via `TryAddEnumerable` — only one instance regardless of how many services are registered. Logs each service start/stop via `[LoggerMessage]` source-generated methods.
- Falls back to `NullLoggerFactory` when `AddLogging()` hasn't been called (e.g. in test scenarios).

### Dependency Graph

```
Pondhawk.Core     (foundation — pipeline infrastructure, utilities, exceptions)
Pondhawk.Watch    (standalone — logging API, Serilog sink, multi-targeted net10.0+netstandard2.0)
Pondhawk.Rules    (standalone)
Pondhawk.Rules.EFCore ──→ Pondhawk.Rules
Pondhawk.Rql      (standalone)
Pondhawk.Hosting  (standalone)
```

## Conventions

- Namespaces match project/folder structure: `Pondhawk.Rules`, `Pondhawk.Rules.Builder`, `Pondhawk.Rules.Evaluation`, `Pondhawk.Rql`, `Pondhawk.Rql.Builder`, `Pondhawk.Rql.Parser`, `Pondhawk.Rql.Serialization`, `Pondhawk.Hosting`
- Exception: `Pondhawk.Core` project uses `RootNamespace=Pondhawk`
- `LangVersion` varies: `default` in Rules and Hosting, `latestmajor` in Rql, `latest` in Watch
