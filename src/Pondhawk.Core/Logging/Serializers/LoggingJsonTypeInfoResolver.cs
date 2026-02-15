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

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Pondhawk.Logging.Serializers;

/// <summary>
/// Custom JSON type info resolver that provides safe property access and sensitive data handling.
/// </summary>
/// <remarks>
/// <para>
/// This resolver wraps all property getters to:
/// <list type="bullet">
/// <item>Catch exceptions thrown by property getters (e.g., MemoryStream.ReadTimeout)</item>
/// <item>Mask sensitive data marked with <see cref="SensitiveAttribute"/></item>
/// </list>
/// </para>
/// <para>
/// System.Text.Json does not provide a built-in way to handle exceptions during
/// property access. Without this resolver, a single throwing property causes the
/// entire serialization to fail. This resolver ensures logging continues even
/// when some properties are inaccessible.
/// </para>
/// </remarks>
internal class LoggingJsonTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return typeInfo;

        foreach (var prop in typeInfo.Properties)
        {
            var sensitive = prop.AttributeProvider switch
            {
                MemberInfo mi => mi.GetCustomAttribute<SensitiveAttribute>(inherit: true),
                ParameterInfo pi => pi.GetCustomAttribute<SensitiveAttribute>(inherit: true),
                _ => null
            };

            if (sensitive is not null)
            {
                var originalGetter = prop.Get;
                prop.Get = o => SensitivePropertyGetter(o, originalGetter, prop.PropertyType);
            }
            else
            {
                var originalGetter = prop.Get;
                prop.Get = o => SafePropertyGetter(o, originalGetter, prop.PropertyType);
            }
        }

        return typeInfo;
    }

    /// <summary>
    /// Safely gets a property value, returning a default if an exception occurs.
    /// </summary>
    private static object? SafePropertyGetter(object source, Func<object, object?>? getter, Type type)
    {
        object? GetDefault()
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        try
        {
            if (getter is null)
                return GetDefault();

            return getter(source);
        }
        catch
        {
            return GetDefault();
        }
    }

    /// <summary>
    /// Gets a property value with sensitive data masking.
    /// </summary>
    /// <remarks>
    /// For string properties, returns "Sensitive - HasValue: true/false" instead of the actual value.
    /// For other types, returns the default value.
    /// </remarks>
    private static object? SensitivePropertyGetter(object source, Func<object, object?>? getter, Type type)
    {
        object? GetDefault()
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        try
        {
            if (getter is null)
                return GetDefault();

            var value = getter(source);

            if (value is string s)
            {
                return $"Sensitive - HasValue: {!string.IsNullOrWhiteSpace(s)}";
            }

            // For non-string sensitive properties, indicate presence without value
            return $"Sensitive - HasValue: {value is not null}";
        }
        catch
        {
            return GetDefault();
        }
    }
}
