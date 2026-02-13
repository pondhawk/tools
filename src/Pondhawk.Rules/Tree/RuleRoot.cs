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

using Pondhawk.Rules.Builder;

namespace Pondhawk.Rules.Tree;

internal sealed class RuleRoot
{
    private readonly Dictionary<Type, RuleNode> _trunk = [];
    private Dictionary<Type, List<RuleNode>> _trunkMatchCache;

    public void Clear()
    {
        foreach( var n in _trunk.Values )
            n.Clear();

        _trunk.Clear();
        _trunkMatchCache = null;
    }

    public void Add( Type[] targetTypes, IEnumerable<IRule> rules )
    {
        // Depth 0: use trunk dictionary
        var target = targetTypes[0];
        if( !_trunk.TryGetValue( target, out var node ) )
        {
            node = new RuleNode { Target = target };
            _trunk[target] = node;
            _trunkMatchCache = null;
        }

        // Depth 1..N-1: use node.GetOrAddBranch
        for( int depth = 1; depth < targetTypes.Length; depth++ )
            node = node.GetOrAddBranch( targetTypes[depth] );

        node.AddRules( rules );
    }

    public bool HasRules( Type[] requestTypes, List<string> namespaces ) =>
        _HasRules( 0, requestTypes, _GetMatchingTrunkNodes( requestTypes[0] ), namespaces );

    public ISet<IRule> FindRules( Type[] requestTypes ) => FindRules( requestTypes, [] );

    public ISet<IRule> FindRules( Type[] requestTypes, List<string> namespaces )
    {
        var sink = new HashSet<IRule>();
        _FindRules( 0, requestTypes, _GetMatchingTrunkNodes( requestTypes[0] ), namespaces, sink );
        return sink;
    }

    private List<RuleNode> _GetMatchingTrunkNodes( Type requestType )
    {
        if( _trunk.Count == 0 )
            return [];

        _trunkMatchCache ??= new();
        if( _trunkMatchCache.TryGetValue( requestType, out var cached ) )
            return cached;

        var matches = new List<RuleNode>();
        foreach( var node in _trunk.Values )
            if( node.Target.IsAssignableFrom( requestType ) )
                matches.Add( node );

        _trunkMatchCache[requestType] = matches;
        return matches;
    }

    private void _FindRules( int depth, Type[] types, List<RuleNode> nodes, List<string> namespaces, HashSet<IRule> sink )
    {
        depth++;

        if( depth == types.Length )
        {
            foreach( var node in nodes )
            {
                if( namespaces.Count == 0 )
                    sink.UnionWith( node.Rules );
                else
                    foreach( var rule in node.Rules )
                        if( namespaces.Contains( rule.Namespace ) )
                            sink.Add( rule );
            }
            return;
        }

        var requestType = types[depth];
        foreach( var node in nodes )
        {
            var matching = node.GetMatchingBranches( requestType );
            if( matching.Count > 0 )
                _FindRules( depth, types, matching, namespaces, sink );
        }
    }

    private bool _HasRules( int depth, Type[] types, List<RuleNode> nodes, List<string> namespaces )
    {
        depth++;

        if( depth == types.Length )
        {
            foreach( var node in nodes )
                if( node.HasRules( namespaces ) )
                    return true;
            return false;
        }

        var requestType = types[depth];
        foreach( var node in nodes )
        {
            var matching = node.GetMatchingBranches( requestType );
            if( matching.Count > 0 && _HasRules( depth, types, matching, namespaces ) )
                return true;
        }

        return false;
    }
}