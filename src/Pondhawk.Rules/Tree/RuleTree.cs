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
using Pondhawk.Rules.Builder;

namespace Pondhawk.Rules.Tree;

/// <summary>
/// Indexes rules by their fact types for fast lookup with polymorphic type matching.
/// </summary>
public sealed class RuleTree : IRuleBase, IRuleSink
{

    private volatile bool _isBuilt;
    private readonly object _buildLock = new();

    private Dictionary<int, RuleRoot> RootMap { get; } = new();

    public int MaxAxisCount => RootMap.Count > 0 ? RootMap.Keys.Max() : 0;


    /// <summary>
    /// Seals the tree and eagerly builds the type-match caches.
    /// After this call, any attempt to add rules will throw.
    /// Called automatically on the first query if not called explicitly.
    /// </summary>
    public void Build()
    {
        if( _isBuilt ) return;
        lock( _buildLock )
        {
            if( _isBuilt ) return;
            foreach( var root in RootMap.Values )
                root.Build();
            _isBuilt = true;
        }
    }

    private void EnsureBuilt()
    {
        if( !_isBuilt ) Build();
    }

    private void ThrowIfBuilt()
    {
        if( _isBuilt )
            throw new InvalidOperationException( "Cannot add rules to a RuleTree after it has been built. All rules must be added before the first evaluation." );
    }


    public bool HasRules(  Type[] factTypes )
    {
        Guard.IsNotNull(factTypes);

        return HasRules( factTypes, new string[] {} );
    }

    public bool HasRules(  Type[] factTypes, IEnumerable<string> namespaces )
    {
        Guard.IsNotNull(factTypes);
        Guard.IsNotNull(namespaces);

        EnsureBuilt();

        RootMap.TryGetValue( factTypes.Length, out var targetRoot );
        if( targetRoot is null )
            return false;

        var ns = namespaces as List<string> ?? namespaces.ToList();
        return targetRoot.HasRules( factTypes, ns );
    }



    public ISet<IRule> FindRules( Type[] factTypes )
    {
        Guard.IsNotNull(factTypes);

        EnsureBuilt();

        RootMap.TryGetValue( factTypes.Length, out var targetRoot );
        return targetRoot is null ? new HashSet<IRule>() : targetRoot.FindRules( factTypes );
    }


    public ISet<IRule> FindRules( Type[] factTypes, IEnumerable<string> namespaces )
    {
        Guard.IsNotNull(factTypes);
        Guard.IsNotNull(namespaces);

        EnsureBuilt();

        RootMap.TryGetValue( factTypes.Length, out var targetRoot );
        if( targetRoot is null )
            return new HashSet<IRule>();

        var ns = namespaces as List<string> ?? namespaces.ToList();
        return targetRoot.FindRules( factTypes, ns );
    }


    public void Add( Type factType, IRule rule )
    {
        ThrowIfBuilt();

        if (!RootMap.TryGetValue(1, out var targetRoot))
        {
            targetRoot = new();
            RootMap[1] = targetRoot;
        }

        targetRoot.Add( [factType], [rule]);

    }


    public void Add(  Type[] factTypes, IEnumerable<IRule> rules )
    {
        ThrowIfBuilt();

        var axisCount = factTypes.Length;

        if( !RootMap.TryGetValue( axisCount, out var targetRoot ) )
        {
            targetRoot = new();
            RootMap[axisCount] = targetRoot;
        }

        targetRoot.Add( factTypes, rules );
    }


    public void Clear()
    {
        foreach( var r in RootMap.Values )
            r.Clear();

        _isBuilt = false;
    }
}