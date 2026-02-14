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

namespace Pondhawk.Rules.Builder;

public class RuleBuilderSource: IRuleBuilderSource
{

    private static Func<Type, bool> Predicate { get; } = t => typeof(IBuilder).IsAssignableFrom(t);

    public void AddTypes( params Assembly[] assemblies )
    {

        ArgumentNullException.ThrowIfNull(assemblies);

        foreach ( var type in assemblies.SelectMany(a=>a.GetTypes()).Where(Predicate) )
            Types.Add(type);
    }

    public void AddTypes( params Type[] types )
    {

        ArgumentNullException.ThrowIfNull(types);

        foreach (var type in types.Where(Predicate))
            Types.Add(type);
    }

    public void AddTypes( IEnumerable<Type> candidates )
    {

        ArgumentNullException.ThrowIfNull(candidates);

        foreach (var type in candidates.Where( Predicate ) )
            Types.Add(type);
    }

    private HashSet<Type> Types { get; } = new ();

    public IEnumerable<Type> GetTypes()
    {
        return Types;
    }

}
