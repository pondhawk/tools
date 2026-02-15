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

using System.Reflection;
using CommunityToolkit.Diagnostics;

namespace Pondhawk.Rules.Builder;

/// <summary>
/// Discovers and collects <see cref="IBuilder"/> types from assemblies or explicit type lists.
/// </summary>
public class RuleBuilderSource : IRuleBuilderSource
{

    private static Func<Type, bool> Predicate { get; } = t => typeof(IBuilder).IsAssignableFrom(t);

    /// <summary>Discovers and adds all <see cref="IBuilder"/> types from the specified assemblies.</summary>
    /// <param name="assemblies">The assemblies to scan for builder types.</param>
    public void AddTypes(params Assembly[] assemblies)
    {

        Guard.IsNotNull(assemblies);

        foreach (var type in assemblies.SelectMany(a => a.GetTypes()).Where(Predicate))
            Types.Add(type);
    }

    /// <summary>Adds the specified types that implement <see cref="IBuilder"/>.</summary>
    /// <param name="types">The types to add.</param>
    public void AddTypes(params Type[] types)
    {

        Guard.IsNotNull(types);

        foreach (var type in types.Where(Predicate))
            Types.Add(type);
    }

    /// <summary>Adds types from the candidate collection that implement <see cref="IBuilder"/>.</summary>
    /// <param name="candidates">The candidate types to filter and add.</param>
    public void AddTypes(IEnumerable<Type> candidates)
    {

        Guard.IsNotNull(candidates);

        foreach (var type in candidates.Where(Predicate))
            Types.Add(type);
    }

    private HashSet<Type> Types { get; } = new();

    /// <inheritdoc />
    public IEnumerable<Type> GetTypes()
    {
        return Types;
    }

}
