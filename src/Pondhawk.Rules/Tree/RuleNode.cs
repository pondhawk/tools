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

internal sealed class RuleNode
{
    private Dictionary<Type, RuleNode> _branches;
    private Dictionary<Type, List<RuleNode>> _branchMatchCache;

    private bool _hasRules;


    private HashSet<string> _namespaces;

    private HashSet<IRule> _rules;
    public Type Target { get; set; }

    private Dictionary<Type, RuleNode> Branches => _branches ??= [];

    private HashSet<string> Namespaces => _namespaces ??= [];

    public HashSet<IRule> Rules => _rules ??= [];

    internal void Build()
    {
        if (_branches is not null && _branches.Count > 0)
        {
            var cache = new Dictionary<Type, List<RuleNode>>(_branches.Count);
            foreach (var requestType in _branches.Keys)
                cache[requestType] = _BuildBranchMatches(requestType);

            _branchMatchCache = cache;

            foreach (var node in _branches.Values)
                node.Build();
        }
    }

    public RuleNode GetOrAddBranch(Type target)
    {
        var branches = Branches;
        if (branches.TryGetValue(target, out var existing))
            return existing;

        var node = new RuleNode { Target = target };
        branches[target] = node;
        return node;
    }

    public List<RuleNode> GetMatchingBranches(Type requestType)
    {
        if (_branches is null || _branches.Count == 0)
            return [];

        if (_branchMatchCache.TryGetValue(requestType, out var cached))
            return cached;

        return _BuildBranchMatches(requestType);
    }

    private List<RuleNode> _BuildBranchMatches(Type requestType)
    {
        var matches = new List<RuleNode>();
        foreach (var node in _branches.Values)
            if (node.Target.IsAssignableFrom(requestType))
                matches.Add(node);
        return matches;
    }

    public bool HasRules(List<string> namespaces)
    {
        if (!_hasRules)
            return false;

        return namespaces.Count == 0 || namespaces.Exists(n => _namespaces.Contains(n));
    }

    public void AddRules(IEnumerable<IRule> source)
    {
        Guard.IsNotNull(source);

        _hasRules = true;
        foreach (IRule r in source)
        {
            Namespaces.Add(r.Namespace);
            Rules.Add(r);
        }
    }


    public void Clear()
    {
        if (_hasRules)
        {
            Namespaces.Clear();
            Rules.Clear();

            _hasRules = false;
        }

        if (_branches is not null)
        {
            foreach (var n in _branches.Values)
                n.Clear();

            _branches.Clear();
        }

        _branchMatchCache = null;
    }
}
