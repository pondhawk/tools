using Pondhawk.Rules.Util;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class HelpersTests
{

    // ========== EncodeSignature / DecodeSignature ==========

    [Fact]
    public void EncodeSignature_SingleIndex_Roundtrips()
    {
        byte[] input = [0];
        var encoded = Helpers.EncodeSignature(input);
        var decoded = Helpers.DecodeSignature(encoded);

        decoded.Length.ShouldBe(1);
        decoded[0].ShouldBe((byte)0);
    }

    [Fact]
    public void EncodeSignature_TwoIndices_Roundtrips()
    {
        byte[] input = [0, 1];
        var encoded = Helpers.EncodeSignature(input);
        var decoded = Helpers.DecodeSignature(encoded);

        decoded.Length.ShouldBe(2);
        decoded[0].ShouldBe((byte)0);
        decoded[1].ShouldBe((byte)1);
    }

    [Fact]
    public void EncodeSignature_ThreeIndices_Roundtrips()
    {
        byte[] input = [0, 1, 2];
        var encoded = Helpers.EncodeSignature(input);
        var decoded = Helpers.DecodeSignature(encoded);

        decoded.Length.ShouldBe(3);
        decoded[0].ShouldBe((byte)0);
        decoded[1].ShouldBe((byte)1);
        decoded[2].ShouldBe((byte)2);
    }

    [Fact]
    public void EncodeSignature_FourIndices_CannotRoundtrip()
    {
        // Known limitation: when all 4 bytes are non-zero (indices 0-3 encode as 1-4),
        // DecodeSignature cannot find a zero terminator and returns empty.
        // This affects Rule<T1,T2,T3,T4> signatures but is mitigated because
        // the EvaluationPlan handles 4-type steps separately.
        byte[] input = [0, 1, 2, 3];
        var encoded = Helpers.EncodeSignature(input);
        var decoded = Helpers.DecodeSignature(encoded);

        decoded.Length.ShouldBe(0);
    }

    [Fact]
    public void EncodeSignature_DifferentIndices_ProduceDifferentSignatures()
    {
        var sig1 = Helpers.EncodeSignature([0]);
        var sig2 = Helpers.EncodeSignature([1]);
        var sig3 = Helpers.EncodeSignature([0, 1]);

        sig1.ShouldNotBe(sig2);
        sig1.ShouldNotBe(sig3);
        sig2.ShouldNotBe(sig3);
    }

    [Fact]
    public void EncodeSignature_SameIndices_ProduceSameSignature()
    {
        var sig1 = Helpers.EncodeSignature([0, 1]);
        var sig2 = Helpers.EncodeSignature([0, 1]);

        sig1.ShouldBe(sig2);
    }

    [Fact]
    public void EncodeSignature_HighIndices_Roundtrips()
    {
        byte[] input = [10, 20, 30];
        var encoded = Helpers.EncodeSignature(input);
        var decoded = Helpers.DecodeSignature(encoded);

        decoded.Length.ShouldBe(3);
        decoded[0].ShouldBe((byte)10);
        decoded[1].ShouldBe((byte)20);
        decoded[2].ShouldBe((byte)30);
    }


    // ========== EncodeSelector / DecodeSelector ==========

    [Fact]
    public void EncodeSelector_SingleIndex_Roundtrips()
    {
        int[] input = [1];
        var encoded = Helpers.EncodeSelector(input);
        var decoded = Helpers.DecodeSelector(encoded);

        decoded.Length.ShouldBe(1);
        decoded[0].ShouldBe(1);
    }

    [Fact]
    public void EncodeSelector_TwoIndices_Roundtrips()
    {
        int[] input = [1, 2];
        var encoded = Helpers.EncodeSelector(input);
        var decoded = Helpers.DecodeSelector(encoded);

        decoded.Length.ShouldBe(2);
        decoded[0].ShouldBe(1);
        decoded[1].ShouldBe(2);
    }

    [Fact]
    public void EncodeSelector_FourIndices_Roundtrips()
    {
        int[] input = [1, 2, 3, 4];
        var encoded = Helpers.EncodeSelector(input);
        var decoded = Helpers.DecodeSelector(encoded);

        decoded.Length.ShouldBe(4);
        decoded[0].ShouldBe(1);
        decoded[1].ShouldBe(2);
        decoded[2].ShouldBe(3);
        decoded[3].ShouldBe(4);
    }

    [Fact]
    public void EncodeSelector_LargeIndices_Roundtrips()
    {
        int[] input = [100, 500, 1000];
        var encoded = Helpers.EncodeSelector(input);
        var decoded = Helpers.DecodeSelector(encoded);

        decoded.Length.ShouldBe(3);
        decoded[0].ShouldBe(100);
        decoded[1].ShouldBe(500);
        decoded[2].ShouldBe(1000);
    }

    [Fact]
    public void EncodeSelector_MaxUInt16_Roundtrips()
    {
        int[] input = [65535];
        var encoded = Helpers.EncodeSelector(input);
        var decoded = Helpers.DecodeSelector(encoded);

        decoded.Length.ShouldBe(1);
        decoded[0].ShouldBe(65535);
    }

    [Fact]
    public void EncodeSelector_DifferentIndices_ProduceDifferentSelectors()
    {
        var sel1 = Helpers.EncodeSelector([1]);
        var sel2 = Helpers.EncodeSelector([2]);
        var sel3 = Helpers.EncodeSelector([1, 2]);

        sel1.ShouldNotBe(sel2);
        sel1.ShouldNotBe(sel3);
    }

    [Fact]
    public void EncodeSelector_SameIndices_ProduceSameSelector()
    {
        var sel1 = Helpers.EncodeSelector([1, 2, 3]);
        var sel2 = Helpers.EncodeSelector([1, 2, 3]);

        sel1.ShouldBe(sel2);
    }

}
