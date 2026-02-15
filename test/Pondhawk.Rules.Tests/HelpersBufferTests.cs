using Pondhawk.Rules.Util;
using Shouldly;
using Xunit;

namespace Pondhawk.Rules.Tests;

public class HelpersBufferTests
{

    // ========== DecodeSelector(long, int[]) buffer overload ==========

    [Fact]
    public void DecodeSelector_Buffer_SingleElement_ReturnsArity1()
    {
        int[] input = [1];
        var encoded = Helpers.EncodeSelector(input);

        var buffer = new int[4];
        var arity = Helpers.DecodeSelector(encoded, buffer);

        arity.ShouldBe(1);
        buffer[0].ShouldBe(1);
    }

    [Fact]
    public void DecodeSelector_Buffer_TwoElements_ReturnsArity2()
    {
        int[] input = [1, 2];
        var encoded = Helpers.EncodeSelector(input);

        var buffer = new int[4];
        var arity = Helpers.DecodeSelector(encoded, buffer);

        arity.ShouldBe(2);
        buffer[0].ShouldBe(1);
        buffer[1].ShouldBe(2);
    }

    [Fact]
    public void DecodeSelector_Buffer_ThreeElements_ReturnsArity3()
    {
        int[] input = [1, 2, 3];
        var encoded = Helpers.EncodeSelector(input);

        var buffer = new int[4];
        var arity = Helpers.DecodeSelector(encoded, buffer);

        arity.ShouldBe(3);
        buffer[0].ShouldBe(1);
        buffer[1].ShouldBe(2);
        buffer[2].ShouldBe(3);
    }

    [Fact]
    public void DecodeSelector_Buffer_FourElements_ReturnsArity4()
    {
        int[] input = [1, 2, 3, 4];
        var encoded = Helpers.EncodeSelector(input);

        var buffer = new int[4];
        var arity = Helpers.DecodeSelector(encoded, buffer);

        arity.ShouldBe(4);
        buffer[0].ShouldBe(1);
        buffer[1].ShouldBe(2);
        buffer[2].ShouldBe(3);
        buffer[3].ShouldBe(4);
    }

    [Fact]
    public void DecodeSelector_Buffer_MatchesAllocatingOverload()
    {
        int[] input = [100, 500, 1000];
        var encoded = Helpers.EncodeSelector(input);

        var allocating = Helpers.DecodeSelector(encoded);

        var buffer = new int[4];
        var arity = Helpers.DecodeSelector(encoded, buffer);

        arity.ShouldBe(allocating.Length);
        for (int i = 0; i < arity; i++)
            buffer[i].ShouldBe(allocating[i]);
    }

    [Fact]
    public void DecodeSelector_Buffer_ZeroSelector_ReturnsArity0()
    {
        var buffer = new int[4];
        var arity = Helpers.DecodeSelector(0L, buffer);

        arity.ShouldBe(0);
    }

    [Fact]
    public void DecodeSelector_Buffer_LargeValues_MatchAllocating()
    {
        int[] input = [65535, 32000, 1, 255];
        var encoded = Helpers.EncodeSelector(input);

        var allocating = Helpers.DecodeSelector(encoded);

        var buffer = new int[4];
        var arity = Helpers.DecodeSelector(encoded, buffer);

        arity.ShouldBe(4);
        for (int i = 0; i < arity; i++)
            buffer[i].ShouldBe(allocating[i]);
    }


    // ========== EncodeSelector(int[], int) buffer with length overload ==========

    [Fact]
    public void EncodeSelector_WithLength_SingleElement_Roundtrips()
    {
        int[] buffer = [1, 0, 0, 0];
        var encoded = Helpers.EncodeSelector(buffer, 1);
        var decoded = Helpers.DecodeSelector(encoded);

        decoded.Length.ShouldBe(1);
        decoded[0].ShouldBe(1);
    }

    [Fact]
    public void EncodeSelector_WithLength_TwoElements_Roundtrips()
    {
        int[] buffer = [1, 2, 0, 0];
        var encoded = Helpers.EncodeSelector(buffer, 2);
        var decoded = Helpers.DecodeSelector(encoded);

        decoded.Length.ShouldBe(2);
        decoded[0].ShouldBe(1);
        decoded[1].ShouldBe(2);
    }

    [Fact]
    public void EncodeSelector_WithLength_IgnoresSlotsBeynondLength()
    {
        int[] buffer = [1, 2, 999, 888];
        var encoded = Helpers.EncodeSelector(buffer, 2);
        var decoded = Helpers.DecodeSelector(encoded);

        decoded.Length.ShouldBe(2);
        decoded[0].ShouldBe(1);
        decoded[1].ShouldBe(2);
    }

    [Fact]
    public void EncodeSelector_WithLength_MatchesArrayOverload()
    {
        int[] source = [10, 20, 30];
        var fromArray = Helpers.EncodeSelector(source);

        int[] buffer = [10, 20, 30, 0];
        var fromBuffer = Helpers.EncodeSelector(buffer, 3);

        fromArray.ShouldBe(fromBuffer);
    }

    [Fact]
    public void EncodeSelector_WithLength_ZeroLength_ReturnsZero()
    {
        int[] buffer = [1, 2, 3, 4];
        var encoded = Helpers.EncodeSelector(buffer, 0);

        encoded.ShouldBe(0L);
    }

    [Fact]
    public void EncodeSelector_Buffer_FullRoundtrip()
    {
        // Encode from buffer with length, decode into buffer
        int[] encodeBuffer = [42, 100, 200, 0];
        var encoded = Helpers.EncodeSelector(encodeBuffer, 3);

        int[] decodeBuffer = new int[4];
        var arity = Helpers.DecodeSelector(encoded, decodeBuffer);

        arity.ShouldBe(3);
        decodeBuffer[0].ShouldBe(42);
        decodeBuffer[1].ShouldBe(100);
        decodeBuffer[2].ShouldBe(200);
    }

}
