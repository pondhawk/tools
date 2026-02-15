// ReSharper disable CollectionNeverUpdated.Local

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pondhawk.Rql.Criteria;

/// <summary>
/// Base class for criteria DTOs. Detects overposted properties via <see cref="System.Text.Json.Serialization.JsonExtensionDataAttribute"/>.
/// </summary>
public class BaseCriteria : ICriteria
{
    /// <inheritdoc />
    public string[]? Rql { get; set; }

    [JsonExtensionData]
    private Dictionary<string, JsonElement> Overposts { get; } = new(StringComparer.Ordinal);

    public bool IsOverposted() => Overposts.Count > 0;

    public IEnumerable<string> GetOverpostNames() => Overposts.Keys;


}
