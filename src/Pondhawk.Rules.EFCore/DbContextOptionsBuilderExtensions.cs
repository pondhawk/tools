using Microsoft.EntityFrameworkCore;

namespace Pondhawk.Rules.EFCore;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder AddRuleValidation(
        this DbContextOptionsBuilder builder,
        IRuleSet ruleSet)
    {
        return builder.AddInterceptors(new RuleValidationInterceptor(ruleSet));
    }
}
