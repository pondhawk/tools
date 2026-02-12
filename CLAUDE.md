# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build the entire solution
dotnet build pondhawk-tools.slnx

# Build a specific project
dotnet build src/Fabrica.Rules/Fabrica.Rules.csproj
dotnet build src/Fabrica.Rql/Fabrica.Rql.csproj
dotnet build src/Fabrica.Rql.Parser/Fabrica.Rql.Parser.csproj
```

**Note:** The solution file (`pondhawk-tools.slnx`) is currently empty (`<Solution />`). Projects may need to be added to it with `dotnet sln add`. No test projects exist yet.

## Project Setup

- **.NET 10** targeting `net10.0` (SDK 10.0.103)
- **Central package management** is enabled in csproj files (`ManagePackageVersionsCentrally=true`) but `Directory.Packages.props` does not exist yet and will need to be created for builds to succeed
- **Nullable reference types** are disabled across all projects
- **External dependency**: All three projects reference `Fabrica.Core` (via `ProjectReference`) which is not present in this repository

## Architecture

This repository contains three class libraries under `src/` that form the **Fabrica** toolkit by Pond Hawk Technologies:

### Fabrica.Rules — Rule Engine

A forward-chaining rule engine with type-based fact matching. Key subsystems:

- **Builder** (`Fabrica.Rules.Builder`): Fluent API for defining rules. `RuleBuilder<TFact1..TFact4>` creates `Rule<T>` instances via `If().And().Then()` chains. Supports up to 4 fact types per rule. Rules have salience (priority), mutex (mutual exclusion), fire-once, inception/expiration.
- **Evaluation** (`Fabrica.Rules.Evaluation`): `EvaluationPlan` generates all fact-type combinations using variations-with-repetition. `TupleEvaluator` executes rules in salience order against fact tuples. `FactSpace` stores facts with int selectors for memory efficiency.
- **Tree** (`Fabrica.Rules.Tree`): `RuleTree` indexes rules by fact types for fast lookup with polymorphic type matching.
- **Validation** (`Fabrica.Rules.Validators`): `ValidationBuilder<TFact>` with `Assert<T>(expr).Is().IsNot().Otherwise()` chains. Runs at very high salience.
- **Factory** (`Fabrica.Rules.Factory`): `RuleSet` for runtime rule creation without predefined builder classes.
- **Listeners** (`Fabrica.Rules.Listeners`): Observer pattern (`IEvaluationListener`) for tracing rule evaluation.

Evaluation flow: `RuleBuilder` → `RuleTree` (indexed by type) → `EvaluationPlan` (generates steps) → `TupleEvaluator` (executes) → `EvaluationResults` (aggregates scores/events/violations). Forward chaining via `InsertFact`/`ModifyFact`/`RetractFact` triggers re-evaluation.

### Fabrica.Rql — Resource Query Language

A filtering DSL with AST, fluent builder, and multiple serialization targets:

- **AST**: `RqlTree` (root) contains `Projection` (field list) and `Criteria` (list of `IRqlPredicate`). `RqlOperator` enum: Equals, NotEquals, LesserThan, GreaterThan, Between, In, NotIn, StartsWith, Contains, etc.
- **Builder** (`Fabrica.Rql.Builder`): `RqlFilterBuilder<TTarget>` provides fluent API: `.Where(expr).Equals(value).And(expr).GreaterThan(value)`. `Introspect()` builds filters from objects decorated with `[CriterionAttribute]`.
- **Serialization** (`Fabrica.Rql.Serialization`): Three output formats:
  - `ToRql()` — RQL text: `(field1,field2) (eq(Name,'John'),gt(Age,30))`
  - `ToLambda<T>()` / `ToExpression<T>()` — compiled LINQ expressions
  - `ToSqlQuery()` / `ToSqlWhere()` — parameterized SQL

### Fabrica.Rql.Parser — RQL Text Parser

Parses RQL text format back into `RqlTree` AST using the **Sprache** parser combinator library. `RqlLanguageParser.ToFilter(string)` parses full format (projections + restrictions); `ToCriteria(string)` parses restrictions only. Value type prefixes: `@` for DateTime, `#` for decimal, `'...'` for strings. Shares `Fabrica.Rql` namespace (`RootNamespace` override in csproj).

### Dependency Graph

```
Fabrica.Rules ──→ Fabrica.Core (external)
Fabrica.Rql ────→ Fabrica.Core (external)
Fabrica.Rql.Parser → Fabrica.Rql → Fabrica.Core (external)
```

## Conventions

- Namespaces match project/folder structure: `Fabrica.Rules`, `Fabrica.Rules.Builder`, `Fabrica.Rules.Evaluation`, `Fabrica.Rql`, `Fabrica.Rql.Builder`, `Fabrica.Rql.Serialization`
- Exception: `Fabrica.Rql.Parser` project uses `RootNamespace=Fabrica.Rql`
- Autofac is used for DI registration (`AutofacExtensions`)
- `LangVersion` varies: `default` in Rules and Rql.Parser, `latestmajor` in Rql
