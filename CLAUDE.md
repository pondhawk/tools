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
```

## Project Setup

- **.NET 10** targeting `net10.0` (SDK 10.0.103)
- **Central package management** via `Directory.Packages.props`
- **Nullable reference types** enabled in Core and Watch projects
- Autofac is used for DI registration (`AutofacExtensions`)

## Architecture

This repository contains class libraries under `src/` that form the **Pondhawk** toolkit by Pond Hawk Technologies:

### Pondhawk.Core — Shared Foundation

Core utilities, logging API, and pipeline infrastructure shared by all Pondhawk projects. Key subsystems:

- **Logging** (`Pondhawk.Logging` namespace): `SerilogExtensions` is the primary Watch logging API with extensions on both `Serilog.ILogger` and `object`:
  - **`object.GetLogger()`** — returns a `Serilog.ILogger` with `SourceContext` set to the concise full name of the object's type (e.g. `MyApp.Services.Repository<Order>`)
  - **`object.EnterMethod()`** — shorthand that calls `GetLogger().EnterMethod()` for method tracing scopes
  - **`ILogger.EnterMethod()`** — creates a disposable method tracing scope with automatic entry/exit logging and elapsed time
  - **`ILogger.Inspect(name, value)`** — logs a name/value pair as `"{Name} = {Value}"` at Debug level
  - **`ILogger.LogObject(value)`** — serializes an object to JSON payload
  - **`ILogger.LogJson/LogSql/LogXml/LogYaml/LogText(title, content)`** — typed payload logging with syntax highlighting hints
  - Also: `WatchSwitchConfig` (switch-based level filtering), `WatchPropertyNames` (Serilog property constants), serializers (`JsonObjectSerializer`), `PayloadType` enum, `[Sensitive]` attribute, `CorrelationManager`.
- **Utilities**: Container helpers (`IRequiresStart`), pipeline infrastructure, process utilities, type extensions.
- **Exceptions**: Common exception types.

### Pondhawk.Watch — Serilog Sink + Watch Infrastructure

A Serilog `ILogEventSink` with Channel-based batching for the Watch structured logging pipeline. Depends on `Pondhawk.Core` for the logging API types.

- **WatchSink**: `ILogEventSink` implementation with unbounded Channel batching. Converts Serilog events to Watch `LogEvent` instances with switch-based filtering.
- **WatchSinkExtensions**: Serilog `LoggerConfiguration` extension method for configuring the Watch sink.
- **Switching**: Dynamic log level control via `ISwitch`/`ISwitchSource` with pattern matching.
- **Sink**: Console, Monitor, and HTTP event sinks with circuit breaker.
- **LogEvent/LogEventBatch**: MemoryPack-serializable event model.

### Pondhawk.Rules — Rule Engine

A forward-chaining rule engine with type-based fact matching. Key subsystems:

- **Builder** (`Pondhawk.Rules.Builder`): Fluent API for defining rules. `RuleBuilder<TFact1..TFact4>` creates `Rule<T>` instances via `If().And().Then()` chains. Supports up to 4 fact types per rule. Rules have salience (priority), mutex (mutual exclusion), fire-once, inception/expiration.
- **Evaluation** (`Pondhawk.Rules.Evaluation`): `EvaluationPlan` generates all fact-type combinations using variations-with-repetition. `TupleEvaluator` executes rules in salience order against fact tuples. `FactSpace` stores facts with int selectors for memory efficiency.
- **Tree** (`Pondhawk.Rules.Tree`): `RuleTree` indexes rules by fact types for fast lookup with polymorphic type matching.
- **Validation** (`Pondhawk.Rules.Validators`): `ValidationBuilder<TFact>` with `Assert<T>(expr).Is().IsNot().Otherwise()` chains. Runs at very high salience.
- **Factory** (`Pondhawk.Rules.Factory`): `RuleSet` for runtime rule creation without predefined builder classes.
- **Listeners** (`Pondhawk.Rules.Listeners`): Observer pattern (`IEvaluationListener`) for tracing rule evaluation.

Evaluation flow: `RuleBuilder` → `RuleTree` (indexed by type) → `EvaluationPlan` (generates steps) → `TupleEvaluator` (executes) → `EvaluationResults` (aggregates scores/events/violations). Forward chaining via `InsertFact`/`ModifyFact`/`RetractFact` triggers re-evaluation.

### Pondhawk.Rql — Resource Query Language

A filtering DSL with AST, fluent builder, parser, and multiple serialization targets:

- **AST**: `RqlTree` (root) contains `Criteria` (list of `IRqlPredicate`). `RqlOperator` enum: Equals, NotEquals, LesserThan, GreaterThan, Between, In, NotIn, StartsWith, Contains, etc.
- **Builder** (`Pondhawk.Rql.Builder`): `RqlFilterBuilder<TTarget>` provides fluent API: `.Where(expr).Equals(value).And(expr).GreaterThan(value)`. `Introspect()` builds filters from objects decorated with `[CriterionAttribute]`.
- **Parser** (`Pondhawk.Rql.Parser`): Parses RQL criteria text back into `RqlTree` AST using the **Sprache** parser combinator library. `RqlLanguageParser.ToCriteria(string)` parses criteria. Value type prefixes: `@` for DateTime, `#` for decimal, `'...'` for strings.
- **Serialization** (`Pondhawk.Rql.Serialization`): Three output formats:
  - `ToRql()` — RQL text: `(eq(Name,'John'),gt(Age,30))`
  - `ToLambda<T>()` / `ToExpression<T>()` — compiled LINQ expressions
  - `ToSqlQuery()` / `ToSqlWhere()` — parameterized SQL

### Dependency Graph

```
Pondhawk.Core (foundation — logging API, Serilog extensions, utilities)
  ↑
Pondhawk.Watch ──→ Pondhawk.Core

Pondhawk.Rules ──→ Pondhawk.Core
Pondhawk.Rql ────→ Pondhawk.Core
```

## Conventions

- Namespaces match project/folder structure: `Pondhawk.Rules`, `Pondhawk.Rules.Builder`, `Pondhawk.Rules.Evaluation`, `Pondhawk.Rql`, `Pondhawk.Rql.Builder`, `Pondhawk.Rql.Parser`, `Pondhawk.Rql.Serialization`
- Exception: `Pondhawk.Core` project uses `RootNamespace=Pondhawk`; logging files use the `Pondhawk.Logging` namespace
- Autofac is used for DI registration (`AutofacExtensions`)
- `LangVersion` varies: `default` in Rules, `latestmajor` in Rql
