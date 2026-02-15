using CommunityToolkit.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Pondhawk.Rules.EFCore;

/// <summary>
/// EF Core <see cref="SaveChangesInterceptor"/> that validates all <c>Added</c> and <c>Modified</c>
/// entities through a <see cref="IRuleSet"/> before they reach the database.
/// Throws <see cref="EntityValidationException"/> when validation fails.
/// </summary>
/// <remarks>
/// <para>Hooks into both <c>SavingChanges</c> and <c>SavingChangesAsync</c> so validation runs for
/// both sync and async save paths. Only entities with state <c>Added</c> or <c>Modified</c> are validated;
/// <c>Deleted</c> and <c>Unchanged</c> entities are skipped.</para>
/// <para>All matching entities are collected into a single <c>Validate()</c> call, so cross-entity
/// validation rules (multi-fact rules) work correctly.</para>
/// <para>Register via <c>optionsBuilder.AddRuleValidation(ruleSet)</c> or manually add to interceptors.</para>
/// </remarks>
public sealed class RuleValidationInterceptor : SaveChangesInterceptor
{
    private readonly IRuleSet _ruleSet;

    /// <summary>
    /// Creates a new interceptor backed by the specified rule set.
    /// </summary>
    /// <param name="ruleSet">The rule set used to validate entities.</param>
    public RuleValidationInterceptor(IRuleSet ruleSet)
    {
        Guard.IsNotNull(ruleSet);
        _ruleSet = ruleSet;
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ValidateEntities(eventData);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
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
