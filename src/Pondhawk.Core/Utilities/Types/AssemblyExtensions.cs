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

public static class AssemblyExtensions
{

    public static Stream? GetResource( this Assembly target, string name )
    {

        Guard.IsNotNullOrEmpty(name);

        return target.GetManifestResourceStream(name);

    }

    public static IEnumerable<string> GetResourceNames( this Assembly target, Func<string, bool> filter )
    {

        Guard.IsNotNull(filter);

        var results = target.GetManifestResourceNames().Where(filter);
        return results;
        
    }

    public static IEnumerable<string> GetResourceNamesByPath( this Assembly target, string path )
    {

        Guard.IsNotNullOrEmpty(path);


        bool Filter(string r) => r.StartsWith(path);

        var results = target.GetManifestResourceNames().Where(Filter);
        return results;

    }    

    public static IEnumerable<string> GetResourceNamesByExt( this Assembly target, string extension )
    {

        Guard.IsNotNullOrEmpty(extension);

        bool Filter(string r) => r.EndsWith(extension);

        var results = target.GetManifestResourceNames().Where( Filter );
        return results;

    }


    public static IEnumerable<string> GetResourceNamesByPathAndExt( this Assembly target, string path, string extension )
    {

        Guard.IsNotNullOrEmpty(path);
        Guard.IsNotNullOrEmpty(extension);

        bool Filter(string r) => r.StartsWith(path) && r.EndsWith(extension);

        var results = target.GetManifestResourceNames().Where( Filter );
        return results;

    }


    public static IEnumerable<Type> GetFilteredTypes( this Assembly target, Func<Type, bool> filter )
    {

        Guard.IsNotNull(filter);


        return target.GetTypes().Where(filter);

    }

    public static IEnumerable<Type> GetImplementations( this Assembly target, Type implements )
    {

        Guard.IsNotNull(implements);


        bool Filter(Type t) => (t != implements) && (implements.IsAssignableFrom(t));

        return target.GetTypes().Where( Filter );

    }


    public static IEnumerable<Type> GetTypesWithAttribute( this Assembly target, Type attribute )
    {

        Guard.IsNotNull(attribute);

        bool Filter(Type t) => t.GetCustomAttributes(attribute, false).Length > 0;

        return target.GetTypes().Where( Filter );

    }

    public static IEnumerable<Type> GetImplementationsWithAttribute( this Assembly target, Type implements, Type attribute )
    {

        Guard.IsNotNull(implements);
        Guard.IsNotNull(attribute);

        bool Predicate(Type t) => (t != implements) && (implements.IsAssignableFrom(t)) && (t.GetCustomAttributes(attribute, false).Length > 0);

        return target.GetTypes().Where( Predicate );

    }


}