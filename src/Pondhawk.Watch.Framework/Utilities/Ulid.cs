/*
The MIT License (MIT)

Copyright (c) 2024 Pond Hawk Technologies Inc.

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

using System;

namespace Pondhawk.Watch.Framework.Utilities
{
    /// <summary>
    /// Simplified ULID generator for netstandard2.0.
    /// Produces timestamp-prefixed unique IDs (Guid-based fallback).
    /// Server treats Uid as an opaque string so exact ULID encoding is not required.
    /// </summary>
    internal static class Ulid
    {
        private static readonly char[] Base32Text = "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToCharArray();
        private static readonly long UnixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        public static string NewUlid()
        {
            var timestampMs = (DateTime.UtcNow.Ticks - UnixEpochTicks) / TimeSpan.TicksPerMillisecond;
            var randomBytes = Guid.NewGuid().ToByteArray();

            // Encode 48-bit timestamp (10 chars) + 80-bit random (16 chars) = 26 chars
            var chars = new char[26];

            // Timestamp: 48 bits = 6 bytes, encoded as 10 base32 chars
            var ts = new byte[6];
            ts[0] = (byte)((timestampMs >> 40) & 0xFF);
            ts[1] = (byte)((timestampMs >> 32) & 0xFF);
            ts[2] = (byte)((timestampMs >> 24) & 0xFF);
            ts[3] = (byte)((timestampMs >> 16) & 0xFF);
            ts[4] = (byte)((timestampMs >> 8) & 0xFF);
            ts[5] = (byte)(timestampMs & 0xFF);

            chars[0] = Base32Text[(ts[0] & 224) >> 5];
            chars[1] = Base32Text[ts[0] & 31];
            chars[2] = Base32Text[(ts[1] & 248) >> 3];
            chars[3] = Base32Text[((ts[1] & 7) << 2) | ((ts[2] & 192) >> 6)];
            chars[4] = Base32Text[(ts[2] & 62) >> 1];
            chars[5] = Base32Text[((ts[2] & 1) << 4) | ((ts[3] & 240) >> 4)];
            chars[6] = Base32Text[((ts[3] & 15) << 1) | ((ts[4] & 128) >> 7)];
            chars[7] = Base32Text[(ts[4] & 124) >> 2];
            chars[8] = Base32Text[((ts[4] & 3) << 3) | ((ts[5] & 224) >> 5)];
            chars[9] = Base32Text[ts[5] & 31];

            // Randomness: use 10 bytes from Guid, encoded as 16 base32 chars
            var r = randomBytes;
            chars[10] = Base32Text[(r[0] & 248) >> 3];
            chars[11] = Base32Text[((r[0] & 7) << 2) | ((r[1] & 192) >> 6)];
            chars[12] = Base32Text[(r[1] & 62) >> 1];
            chars[13] = Base32Text[((r[1] & 1) << 4) | ((r[2] & 240) >> 4)];
            chars[14] = Base32Text[((r[2] & 15) << 1) | ((r[3] & 128) >> 7)];
            chars[15] = Base32Text[(r[3] & 124) >> 2];
            chars[16] = Base32Text[((r[3] & 3) << 3) | ((r[4] & 224) >> 5)];
            chars[17] = Base32Text[r[4] & 31];
            chars[18] = Base32Text[(r[5] & 248) >> 3];
            chars[19] = Base32Text[((r[5] & 7) << 2) | ((r[6] & 192) >> 6)];
            chars[20] = Base32Text[(r[6] & 62) >> 1];
            chars[21] = Base32Text[((r[6] & 1) << 4) | ((r[7] & 240) >> 4)];
            chars[22] = Base32Text[((r[7] & 15) << 1) | ((r[8] & 128) >> 7)];
            chars[23] = Base32Text[(r[8] & 124) >> 2];
            chars[24] = Base32Text[((r[8] & 3) << 3) | ((r[9] & 224) >> 5)];
            chars[25] = Base32Text[r[9] & 31];

            return new string(chars);
        }
    }
}
