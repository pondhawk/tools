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

using CommunityToolkit.Diagnostics;

namespace Pondhawk.Utilities.Types;

/// <summary>
/// Extension methods for byte arrays, DateTime formatting, and human-readable type name generation.
/// </summary>
public static class TypeExtensions
{


    /// <summary>
    /// Converts a byte array to its lowercase hexadecimal string representation.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>A lowercase hexadecimal string.</returns>
    public static string ToHexString(this byte[] bytes)
    {

        Guard.IsNotNull(bytes);

        var hex = Convert.ToHexStringLower(bytes);
        return hex;

    }


    /// <summary>
    /// Converts a <see cref="DateTime"/> to a sortable UTC timestamp string in the format YYYYMMDDTicks.
    /// </summary>
    /// <param name="source">The <see cref="DateTime"/> to convert.</param>
    /// <returns>A sortable timestamp string.</returns>
    public static string ToTimestampString(this DateTime source)
    {

        var utc = source.ToUniversalTime();

        var y = utc.Year.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(4, '0');
        var m = utc.Month.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(2, '0');
        var d = utc.Day.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(2, '0');
        var t = utc.TimeOfDay.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(20, '0');

        var ts = string.Join("", y, m, d, t);
        return ts;

    }

    /// <summary>
    /// Gets a human-readable short name for a type, including generic type arguments (e.g. <c>Repository&lt;Order&gt;</c>).
    /// </summary>
    /// <param name="type">The type to get the concise name for.</param>
    /// <returns>A concise type name with generic arguments expanded.</returns>
    public static string GetConciseName(this Type type)
    {

        var conciseName = type.Name;
        if (!type.IsGenericType)
            return conciseName;

        var iBacktick = conciseName.IndexOf('`');
        if (iBacktick > 0) conciseName =
            conciseName.Remove(iBacktick);

        var genericParameters = type.GetGenericArguments().Select(x => x.GetConciseName());
        conciseName += "<" + string.Join(", ", genericParameters) + ">";


        return conciseName;


    }

    /// <summary>
    /// Gets a human-readable fully qualified name for a type, including generic type arguments (e.g. <c>MyApp.Services.Repository&lt;Order&gt;</c>).
    /// </summary>
    /// <param name="type">The type to get the concise full name for.</param>
    /// <returns>A concise fully qualified type name with generic arguments expanded, or an empty string if the full name is unavailable.</returns>
    public static string GetConciseFullName(this Type type)
    {

        var conciseName = type.FullName;
        if (string.IsNullOrWhiteSpace(conciseName))
            return "";

        if (!type.IsGenericType)
            return conciseName;

        var iBacktick = conciseName.IndexOf('`');
        if (iBacktick > 0) conciseName =
            conciseName.Remove(iBacktick);

        var genericParameters = type.GetGenericArguments().Select(x => x.GetConciseName());
        conciseName += "<" + string.Join(", ", genericParameters) + ">";


        return conciseName;


    }



}
