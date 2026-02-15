using Microsoft.EntityFrameworkCore;

namespace Pondhawk.Rules.EFCore;

/// <summary>
/// Extension methods for adding rule-based entity validation to an EF Core <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="RuleValidationInterceptor"/> that validates all added and modified entities
    /// through the specified <paramref name="ruleSet"/> before <c>SaveChanges</c>.
    /// </summary>
    /// <param name="builder">The options builder.</param>
    /// <param name="ruleSet">The rule set used to validate entities.</param>
    public static DbContextOptionsBuilder AddRuleValidation(
        this DbContextOptionsBuilder builder,
        IRuleSet ruleSet)
    {
        return builder.AddInterceptors(new RuleValidationInterceptor(ruleSet));
    }
}
