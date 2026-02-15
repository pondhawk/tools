# Pondhawk.Rules.EFCore

Pre-save entity validation interceptor for EF Core powered by the Pondhawk.Rules engine. Validates all `Added` and `Modified` entities through your rule set before they reach the database.

## Quick Start

### Configure Validation

```csharp
using Pondhawk.Rules.EFCore;
using Pondhawk.Rules.Factory;

// Define validation rules
var ruleSet = new RuleSet();

ruleSet.AddValidation<Order>("total-positive")
    .Assert<decimal>(o => o.Total)
    .IsGreaterThan(0m);

ruleSet.AddValidation<Order>("items-required")
    .AssertOver<OrderItem>(o => o.Items)
    .IsNotEmpty();

// Wire into DbContext
var options = new DbContextOptionsBuilder<AppContext>()
    .UseSqlServer(connectionString)
    .AddRuleValidation(ruleSet)
    .Options;
```

### Automatic SaveChanges Validation

```csharp
var context = new AppContext(options);

var order = new Order { Total = 100m, Items = [new OrderItem()] };
context.Orders.Add(order);

await context.SaveChangesAsync();
// Interceptor validates all Added/Modified entities
// Throws EntityValidationException if violations exist
```

### Handle Validation Failures

```csharp
try
{
    await context.SaveChangesAsync();
}
catch (EntityValidationException ex)
{
    var violations = ex.ValidationResult.Violations;
    // violations contains the structured RuleEvent violations
}
```

## How It Works

- `RuleValidationInterceptor` extends `SaveChangesInterceptor`.
- On `SavingChanges`/`SavingChangesAsync`, it pulls all `Added` and `Modified` entities from `ChangeTracker`.
- Each entity is validated through `IRuleSet.Validate()`.
- If any entity fails validation, `EntityValidationException` is thrown before the save reaches the database.
