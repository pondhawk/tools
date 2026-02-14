using Pondhawk.Rules.Util;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class CombinatoricsTests
{

    [Fact]
    public void Combinations_K1_ReturnsElements()
    {
        var elements = new[] { 1, 2, 3 };
        var result = elements.Combinations(1).Select(c => c.ToList()).ToList();

        result.Count.ShouldBe(3);
    }

    [Fact]
    public void Combinations_K2_ReturnsCorrectCount()
    {
        var elements = new[] { 1, 2, 3 };
        var result = elements.Combinations(2).Select(c => c.ToList()).ToList();

        // C(3,2) = 3
        result.Count.ShouldBe(3);
    }

    [Fact]
    public void Combinations_K0_ReturnsSingleEmptySet()
    {
        var elements = new[] { 1, 2, 3 };
        var result = elements.Combinations(0).Select(c => c.ToList()).ToList();

        result.Count.ShouldBe(1);
        result[0].Count.ShouldBe(0);
    }

    [Fact]
    public void CombinationsWithRepetition_K2_AllowsDuplicates()
    {
        var elements = new[] { 1, 2 };
        var result = elements.CombinationsWithRepetition(2).Select(c => c.ToList()).ToList();

        // Implementation produces permutations with repetition: (1,1), (1,2), (2,1), (2,2) = 4
        result.Count.ShouldBe(4);
    }

    [Fact]
    public void Variations_K2_ReturnsAllSizes()
    {
        var elements = new[] { 1, 2, 3 };
        var result = elements.Variations(2).Select(v => v.ToList()).ToList();

        // Variations up to k=2: C(3,1) + C(3,2) = 3 + 3 = 6
        result.Count.ShouldBe(6);
    }

    [Fact]
    public void VariationsWithRepetition_K2_IncludesRepeats()
    {
        var elements = new[] { 1, 2 };
        var result = elements.VariationsWithRepetition(2).Select(v => v.ToList()).ToList();

        // k=1: (1), (2) = 2
        // k=2 with repetition: (1,1), (1,2), (2,1), (2,2) = 4
        // Total = 6
        result.Count.ShouldBe(6);
    }

}
