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

namespace Pondhawk.Utilities.Types;

/// <summary>
/// Collects and filters types from assemblies for discovery-based registration patterns.
/// </summary>
public class TypeSource
{

    private static Func<Type, bool> DefaultPredicate { get; } = _ => true;

    /// <summary>
    /// Gets the predicate used to filter types when adding them to the source. Override to customize filtering.
    /// </summary>
    /// <returns>A predicate that returns <c>true</c> for types that should be included.</returns>
    protected virtual Func<Type, bool> GetPredicate()
    {
        return DefaultPredicate;
    }


    /// <summary>
    /// Adds all types from the specified assemblies that pass the predicate filter.
    /// </summary>
    /// <param name="assemblies">The assemblies whose types should be added.</param>
    public void AddTypes(params Assembly[] assemblies)
    {

        Guard.IsNotNull(assemblies);

        foreach (var type in assemblies.SelectMany(a => a.GetTypes()).Where(GetPredicate()))
            Types.Add(type);
    }


    /// <summary>
    /// Adds the specified types that pass the predicate filter.
    /// </summary>
    /// <param name="types">The types to add.</param>
    public void AddTypes(params Type[] types)
    {

        Guard.IsNotNull(types);

        foreach (var type in types.Where(GetPredicate()))
            Types.Add(type);
    }


    /// <summary>
    /// Adds the specified candidate types that pass the predicate filter.
    /// </summary>
    /// <param name="candidates">The candidate types to evaluate and add.</param>
    public void AddTypes(IEnumerable<Type> candidates)
    {

        Guard.IsNotNull(candidates);

        foreach (var type in candidates.Where(GetPredicate()))
            Types.Add(type);
    }


    private HashSet<Type> Types { get; } = new();

    /// <summary>
    /// Returns all types that have been collected by this source.
    /// </summary>
    /// <returns>An enumerable of the collected types.</returns>
    public IEnumerable<Type> GetTypes()
    {
        return Types;
    }


}
