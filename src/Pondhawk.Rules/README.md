# Pondhawk.Rules

A forward-chaining rule engine with type-based fact matching, validation, and weighted decision support. Standalone -- no dependency on other Pondhawk packages.

## Quick Start

### Define and Evaluate Rules

```csharp
using Pondhawk.Rules.Factory;

var ruleSet = new RuleSet();

ruleSet.AddRule<Person>("age-check")
    .When(p => p.Age >= 18)
    .Then(p => p.Status = "Adult");

ruleSet.AddRule<Person>("multi-condition")
    .When(p => p.Age >= 18)
    .And(p => p.IsActive)
    .Then(p => p.CanVote = true);

var result = ruleSet.Evaluate(new Person { Name = "Alice", Age = 25 });
// result.TotalFired, result.Events, result.Score
```

### Validation

```csharp
ruleSet.AddValidation<Person>("name-required")
    .Assert(p => p.Name)
    .Required();

ruleSet.AddValidation<Person>("age-range")
    .Assert(p => p.Age)
    .IsGreaterThanOrEqual(0)
    .IsLessThanOrEqual(150);

bool valid = ruleSet.TryValidate(person, out var violations);
```

### Per-Fact Conditions and Pattern Matching

Multi-fact rules support per-fact `When()`/`And()` overloads that accept a single-fact predicate, enabling C# pattern matching with full type inference:

```csharp
ruleSet.AddRule<Person, Order>("gold-discount")
    .When((Func<Person, bool>)(p => p.Tier is "Gold" or "Platinum"))
    .And((Func<Order, bool>)(o => o.Total is > 200 and < 10_000))
    .Then((p, o) => o.DiscountPct = 0.10m);
```

When the two fact types are the same, use the combined overload: `.When((f1, f2) => ...)`.

### Scoring / Decisions

```csharp
ruleSet.AddRule<Application>("credit-check")
    .When(a => a.CreditScore > 700)
    .FireAffirm(10);

ruleSet.AddRule<Application>("bankruptcy")
    .When(a => a.HasBankruptcy)
    .FireVeto(20);

bool approved = ruleSet.Decide(application); // Score >= 0
```

### Class-Based Rule Builders

For organized, reusable rule sets, extend `RuleBuilder<T>`:

```csharp
public class OrderRules : RuleBuilder<Order>
{
    public OrderRules()
    {
        Rule()
            .When(o => o.Total > 1000)
            .Then(o => o.RequiresApproval = true);

        Rule()
            .When(o => o.Items.Count == 0)
            .Then(o => o.Status = "Empty");
    }
}
```

### Class-Based Validation Builders

For organized validation, extend `ValidationBuilder<T>`:

```csharp
public class PersonValidation : ValidationBuilder<Person>
{
    public PersonValidation()
    {
        Assert(p => p.Name).Required();
        Assert(p => p.Email).Required().IsEmail();
        Assert(p => p.Age).IsGreaterThanOrEqual(0);
    }
}
```

### DI Registration

```csharp
services.UseRules(typeof(OrderRules).Assembly);
```

## Authoring Guidelines

**Avoid `||` (OR) in conditions.** Each OR branch should be a separate rule. The engine evaluates all matching rules independently, so OR logic is expressed by writing two rules with the same consequence:

```csharp
// Bad -- OR inside a condition
ruleSet.AddRule<Person>("discount")
    .When(p => p.Tier == "Gold" || p.Tier == "Platinum")
    .Then(p => p.DiscountPct = 0.10m);

// Good -- two rules, one per branch
ruleSet.AddRule<Person>("gold-discount")
    .When(p => p.Tier == "Gold")
    .Then(p => p.DiscountPct = 0.10m);

ruleSet.AddRule<Person>("platinum-discount")
    .When(p => p.Tier == "Platinum")
    .Then(p => p.DiscountPct = 0.10m);
```

This keeps rules atomic, independently traceable in `EvaluationResults`, and composable with salience, mutex, and fire-once controls.

**Use `And()` for multiple conditions, not `&&`.** Chaining `.And()` gives each condition its own predicate, improving readability and traceability:

```csharp
// Prefer
ruleSet.AddRule<Person>("eligible")
    .When(p => p.Age >= 18)
    .And(p => p.IsActive)
    .Then(p => p.CanVote = true);
```

## Key Concepts

- **Facts**: Objects inserted into the evaluation context. Rules match against fact types.
- **Forward Chaining**: When a rule modifies a fact (`InsertFact`/`ModifyFact`/`RetractFact`), evaluation re-runs to account for changes.
- **Salience**: Priority ordering. Higher salience rules fire first (default: 500, validations: 100000+).
- **Mutex**: Only the first matching rule in a mutex group fires.
- **Fire-Once**: A rule fires at most once per fact tuple per evaluation.
- **Inception/Expiration**: Time-based rule activation windows.
- **EvaluationContext**: Holds facts, results, lookup tables, and configuration for a single evaluation session.
- **EvaluationResults**: Aggregates events, scores (Affirm/Veto), timing, and fired rule statistics.

## Validation Extensions

Built-in validators for common types:

| Type | Extensions |
|------|-----------|
| `string` | `Required`, `IsEmail`, `MinLength`, `MaxLength`, `Matches`, `StartsWith`, `Contains` |
| `int/long/decimal/double` | `IsGreaterThan`, `IsLessThan`, `IsBetween`, `IsEqualTo`, `IsPositive`, `IsNegative` |
| `DateTime` | `Required`, `IsInFuture`, `IsInPast`, `IsBetween`, `IsToday`, `IsWeekday` |
| `bool` | `IsTrue`, `IsFalse` |
| `Enum` | `IsInEnum`, `IsEnumName` |
| `IEnumerable<T>` | `Required`, `IsEmpty`, `IsNotEmpty`, `Has`, `HasNone`, `HasCount`, `HasCountBetween` |

## Exceptions

- `ViolationsExistException` -- Evaluation produced violation events (default: thrown automatically).
- `NoRulesEvaluatedException` -- No rules matched the given facts.
- `EvaluationExhaustedException` -- Exceeded max evaluations or max duration (possible circular rules).

Call `context.SuppressExceptions()` to collect results without throwing.
