using Pondhawk.Rules;

namespace Pondhawk.Rules.EFCore;

/// <summary>
/// Thrown when EF Core entity validation fails. Carries the <see cref="Pondhawk.Rules.ValidationResult"/>
/// containing all violations.
/// </summary>
public sealed class EntityValidationException : Exception
{
    /// <summary>
    /// The structured validation result containing violations grouped by category.
    /// </summary>
    public ValidationResult ValidationResult { get; }

    public EntityValidationException(ValidationResult validationResult)
        : base(FormatMessage(validationResult))
    {
        ValidationResult = validationResult;
    }

    private static string FormatMessage(ValidationResult validationResult)
    {
        var violations = validationResult.Violations;

        if (violations.Count == 0)
            return "Entity validation failed.";

        if (violations.Count == 1)
            return $"Entity validation failed: {violations[0].Message}";

        return $"Entity validation failed with {violations.Count} violations: " +
               string.Join("; ", violations.Select(v => v.Message));
    }
}
