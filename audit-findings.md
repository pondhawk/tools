# Pondhawk Solution — Code Audit Findings

Audit performed: 2026-02-14

---

## Pondhawk.Core

| # | Severity | Issue | Location |
|---|----------|-------|----------|
| C1 | ~~**Critical**~~ **Fixed** | ~~`ArgumentException.ThrowIfNullOrEmpty(nameof(name))` validates the literal string "name" instead of the variable — validation is a no-op~~ Replaced with `Guard.IsNotNullOrEmpty(name)` across all 5 occurrences | `AssemblyExtensions.cs` |
| C3 | ~~**Critical**~~ **Fixed** | ~~`FileSignalController.StartAsync` fires `Task.Run()` without storing/awaiting the task — exceptions are unobservable~~ Task stored in `_watchTask` field; `Dispose()` now waits for completion | `FileSignalController.cs` |
| C4 | **High** | `SerilogExtensions.Default` is a static mutable property with no synchronization — race condition | `SerilogExtensions.cs:12` |
| C5 | **High** | `WatchSwitchConfig.IsEnabledFunc` static field read/written without volatile or lock | `WatchSwitchConfig.cs:7,11` |
| C6 | ~~**High**~~ **Fixed** | ~~`FileSignalController.Dispose()` only sets `EndWatchEvent` — 3 other `ManualResetEvent` instances are never disposed~~ All 4 `ManualResetEvent` instances now disposed after watch task completes | `FileSignalController.cs` |
| C7 | **Medium** | `DateTimeHelpers.RecentModels` allocates a new `List` on every property access — should be cached | `DateTimeHelpers.cs:63` |
| C8 | **Medium** | `Ok` class and `EmptyDetails` field appear unused | `Error.cs:5-10,15` |
| C9 | **Low** | Potentially unused packages: `YamlDotNet`, `Microsoft.Extensions.Configuration`, `Autofac.Extensions.DependencyInjection` | `Pondhawk.Core.csproj` |

---

## Pondhawk.Watch

| # | Severity | Issue | Location |
|---|----------|-------|----------|
| W1 | ~~**Critical**~~ **Fixed** | ~~`_disposed` flag in `WatchSink` not volatile — race between `Emit()` and `Dispose()` can lose events or use-after-dispose~~ Changed to `int` with `Volatile.Read` in `Emit()` and `Interlocked.Exchange` in `Dispose()`/`DisposeAsync()` | `WatchSink.cs` |
| W2 | **High** | `WatchSwitchSource.UpdateAsync` catches ALL exceptions silently — hides critical failures | `WatchSwitchSource.cs:122-126` |
| W3 | **High** | `FlushCriticalBuffer` calls `Insert(0, e)` in a loop — O(n^2) for each flush | `WatchSink.cs:275-281` |
| W4 | **High** | `ConcurrentQueue.Count` check in `BufferCriticalEvents` is not atomic with `TryDequeue` — buffer can exceed max | `WatchSink.cs:262` |
| W5 | **Medium** | `Switch` properties are mutable after construction despite being conceptually immutable config | `Switch.cs:38-117` |
| W6 | **Medium** | `WatchSwitchSource` implements `IAsyncDisposable` but not `IDisposable` | `WatchSwitchSource.cs:43` |
| W7 | **Low** | Unused packages: `JetBrains.Annotations`, possibly `Microsoft.Extensions.Http.Resilience` | `Pondhawk.Watch.csproj` |

---

## Pondhawk.Rules

| # | Severity | Issue | Location |
|---|----------|-------|----------|
| R1 | **High** | `EvaluationPlan` caches (`CachedVariations`, `CachedTypeCount`) are shared mutable state without synchronization | `EvaluationPlan.cs:64-65,168-170` |
| R2 | ~~**High**~~ **Fixed** | ~~`RuleTree._isBuilt` flag in double-checked locking should be `volatile` for correct memory ordering~~ Already declared `volatile` — finding was stale | `RuleTree.cs` |
| R3 | **Medium** | `EvaluationResults.Events`, `Shared`, `FiredRules` expose mutable collections — callers can corrupt state | `EvaluationResults.cs:46,56,71` |
| R4 | **Medium** | Several public classes (`RuleSet`, `ForeachRule`, `Rule`) should be sealed | Multiple files |
| R5 | **Medium** | Unsafe casts from `object[]` without type checks in `ForeachRule` and `Rule` | Multiple files |
| R6 | **Low** | `len` variables assigned but only used once — minor dead code | `Rule.cs` multiple lines |

---

## Pondhawk.Rql

| # | Severity | Issue | Location |
|---|----------|-------|----------|
| Q1 | ~~**Critical**~~ **Fixed** | ~~SQL injection — string values with single quotes not escaped in `ToSqlQuery()`~~ `In`/`NotIn` enumeration branch now uses parameterized placeholders like all other operators | `SqlSerializerExtensions.cs` |
| Q2 | ~~**Critical**~~ **Fixed** | ~~`Between` operator accesses `Values[1]` without bounds check — crash on malformed predicates~~ Added `Values.Count < 2` guard with descriptive `RqlException` in both serializers | `LambdaSerializerExtensions.cs`, `SqlSerializerExtensions.cs` |
| Q3 | ~~**High**~~ **Fixed** | ~~Parser substring operations assume length > 1 — crash on edge inputs like `#` or `'`~~ Added length guards for empty strings, `@`/`#` prefixes, and single-quote pairs before substring calls | `RqlLanguageParser.cs` |
| Q4 | **High** | `RqlPredicate._cachedValues` not invalidated when `Values` list is modified directly (only via `Value` setter) | `RqlPredicate.cs:29,71,79` |
| Q5 | **High** | `Expression.Property(entity, name)` — no validation that property exists on target type | `LambdaSerializerExtensions.cs:121` |
| Q6 | **Medium** | `IConvertible.ToType()` can throw `InvalidCastException`/`OverflowException` — uncaught | `SqlSerializerExtensions.cs:220,232,239` |
| Q7 | **Medium** | No validation that `Between` has exactly 2 values anywhere in builder or parser | Parser + Builder |
| Q8 | ~~**Medium**~~ **Fixed** | ~~Missing string escape in RQL serialization — round-trip failures if strings contain single quotes~~ Serializer now doubles single quotes (`'` → `''`); parser unescapes on read | `RqlSerializerExtensions.cs`, `RqlLanguageParser.cs` |

---

## Pondhawk.Hosting

| # | Severity | Issue | Location |
|---|----------|-------|----------|
| H1 | ~~**High**~~ **Fixed** | ~~Unguarded exceptions in `StartAsync` — if one service fails, remaining services never start, no error logging~~ Each service start wrapped in try/catch with error logging; remaining services continue | `ServiceStarterHostedService.cs` |
| H2 | ~~**High**~~ **Fixed** | ~~Unguarded exceptions in `StopAsync` — if one service fails to stop, remaining services are abandoned~~ Each service stop wrapped in try/catch with error logging; remaining services continue | `ServiceStarterHostedService.cs` |
| H3 | **Medium** | Implicit transitive dependencies on `Microsoft.Extensions.Logging` and `Microsoft.Extensions.DependencyInjection` — not declared in csproj | `Pondhawk.Hosting.csproj` |
| H4 | **Low** | Inconsistent logging: `StartAsync` logs warning when service not resolved, `StopAsync` silently skips | `ServiceStarterHostedService.cs:32-36 vs 56-57` |

---

## Pondhawk.Rules.EFCore

| # | Severity | Issue | Location |
|---|----------|-------|----------|
| E1 | **High** | Shared `IRuleSet` instance in singleton interceptor — thread safety depends on whether `IRuleSet` is thread-safe for concurrent `Validate()` calls. Each `Validate()` creates its own `EvaluationContext` so likely safe, but `RuleTree` read-path needs to be verified as read-only after `Lazy<T>` init. | `RuleValidationInterceptor.cs:8-12` |
| E2 | **Medium** | `SavingChangesAsync` calls synchronous `ValidateEntities()` — blocks the async path, ignores `CancellationToken` | `RuleValidationInterceptor.cs:23-29` |
| E3 | ~~**Low**~~ **Fixed** | ~~`RuleValidationInterceptor` and `EntityValidationException` should be sealed~~ Both classes sealed | Both files |
