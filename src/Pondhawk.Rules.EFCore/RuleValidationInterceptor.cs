using CommunityToolkit.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Pondhawk.Rules.EFCore;

public sealed class RuleValidationInterceptor : SaveChangesInterceptor
{
    private readonly IRuleSet _ruleSet;

    public RuleValidationInterceptor(IRuleSet ruleSet)
    {
        Guard.IsNotNull(ruleSet);
        _ruleSet = ruleSet;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ValidateEntities(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ValidateEntities(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ValidateEntities(DbContextEventData eventData)
    {
        var context = eventData.Context;
        if (context is null)
            return;

        var entities = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity)
            .ToArray();

        if (entities.Length == 0)
            return;

        var validationResult = _ruleSet.Validate(entities);

        if (!validationResult.IsValid)
            throw new EntityValidationException(validationResult);
    }
}
