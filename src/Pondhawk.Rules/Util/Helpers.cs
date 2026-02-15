/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


namespace Pondhawk.Rules.Util;

internal static class Helpers
{
    internal static int EncodeSignature(byte[] signatureIndices)
    {
        int signature = 0;
        for (int i = 0; i < signatureIndices.Length; i++)
            signature |= (signatureIndices[i] + 1) << (i * 8);
        return signature;
    }


    internal static byte[] DecodeSignature(int signature)
    {
        int b0 = signature & 0xFF;
        int b1 = (signature >> 8) & 0xFF;
        int b2 = (signature >> 16) & 0xFF;
        int b3 = (signature >> 24) & 0xFF;

        int len = b0 == 0 ? 0 : b1 == 0 ? 1 : b2 == 0 ? 2 : b3 == 0 ? 3 : 4;

        var signatureIndices = new byte[len];
        if (len > 0) signatureIndices[0] = (byte)(b0 - 1);
        if (len > 1) signatureIndices[1] = (byte)(b1 - 1);
        if (len > 2) signatureIndices[2] = (byte)(b2 - 1);
        if (len > 3) signatureIndices[3] = (byte)(b3 - 1);

        return signatureIndices;
    }


    internal static long EncodeSelector(int[] selectorIndices)
    {
        long selector = 0;
        for (int i = 0; i < selectorIndices.Length; i++)
            selector |= (long)(ushort)selectorIndices[i] << (i * 16);
        return selector;
    }


    internal static int[] DecodeSelector(long selector)
    {
        int v0 = (int)(selector & 0xFFFF);
        int v1 = (int)((selector >> 16) & 0xFFFF);
        int v2 = (int)((selector >> 32) & 0xFFFF);
        int v3 = (int)((selector >> 48) & 0xFFFF);

        int len = v0 == 0 ? 0 : v1 == 0 ? 1 : v2 == 0 ? 2 : v3 == 0 ? 3 : 4;

        var selectorIndices = new int[len];
        if (len > 0) selectorIndices[0] = v0;
        if (len > 1) selectorIndices[1] = v1;
        if (len > 2) selectorIndices[2] = v2;
        if (len > 3) selectorIndices[3] = v3;

        return selectorIndices;
    }


    internal static int DecodeSelector(long selector, int[] buffer)
    {
        buffer[0] = (int)(selector & 0xFFFF);
        buffer[1] = (int)((selector >> 16) & 0xFFFF);
        buffer[2] = (int)((selector >> 32) & 0xFFFF);
        buffer[3] = (int)((selector >> 48) & 0xFFFF);

        return buffer[0] == 0 ? 0 : buffer[1] == 0 ? 1 : buffer[2] == 0 ? 2 : buffer[3] == 0 ? 3 : 4;
    }


    internal static long EncodeSelector(int[] buffer, int length)
    {
        long selector = 0;
        for (int i = 0; i < length; i++)
            selector |= (long)(ushort)buffer[i] << (i * 16);
        return selector;
    }
}
