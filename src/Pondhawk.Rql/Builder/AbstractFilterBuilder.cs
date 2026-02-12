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


// ReSharper disable UnusedMember.Global

using System.Reflection;
using CommunityToolkit.Diagnostics;
using Pondhawk.Rql.Criteria;

namespace Pondhawk.Rql.Builder;

public abstract class AbstractFilterBuilder<TBuilder>: IRqlFilter where TBuilder: AbstractFilterBuilder<TBuilder>
{



    public static implicit operator List<IRqlPredicate>(  AbstractFilterBuilder<TBuilder> builder )
    {
        var list = new List<IRqlPredicate>();
        list.AddRange( builder.Predicates );
        return list;
    }

    protected AbstractFilterBuilder()
    {

        Predicates = new List<IRqlPredicate>();

    }

    protected AbstractFilterBuilder( RqlTree tree ) : this()
    {

        CurrentName = "";
        Predicates  = new List<IRqlPredicate>(tree.Criteria);

    }



    public abstract Type Target { get; }

    public bool Is<TTarget>()
    {

        var result = (Target == typeof(TTarget)) || (Target.IsAssignableFrom( typeof(TTarget) )) || (typeof(TTarget).IsAssignableFrom( Target ));

        return result;

    }


    #region Criteria related members

    public TBuilder Introspect(  ICriteria source, IDictionary<string,string> map=null )
    {

        Guard.IsNotNull(source);


        var parts = new Dictionary<string, RqlPredicate>();

        foreach (var prop in source.GetType().GetProperties())
        {


            if( prop.GetCustomAttribute(typeof(CriterionAttribute)) is not CriterionAttribute attr )
                continue;


            if (!prop.CanRead)
                continue;


            var value = prop.GetValue(source);
            if( value == null )
                continue;



            var includeMethod = source.GetType().GetMethod($"Include{prop.Name}");
            if (includeMethod is not null)
            {
                var ret = includeMethod.Invoke(source, new object[] { });
                if (ret is false)
                    continue;
            }
            else
            {


                if (prop.PropertyType == typeof(ICollection<string>) && ((ICollection<string>)value).Count == 0)
                    continue;
                if (prop.PropertyType == typeof(ICollection<int>) && ((ICollection<int>)value).Count == 0)
                    continue;
                if (prop.PropertyType == typeof(ICollection<long>) && ((ICollection<long>)value).Count == 0)
                    continue;

            }


            var target   = string.IsNullOrWhiteSpace(attr.Name) ? prop.Name : attr.Name;
            var dataType = prop.PropertyType;

            if (dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(Nullable<>))
                dataType = Nullable.GetUnderlyingType(dataType);


            switch (attr.Operand)
            {
                case OperandKind.Single:
                case OperandKind.From:
                case OperandKind.To:
                    break;
                case OperandKind.List:
                    dataType = typeof(string);
                    break;
                case OperandKind.ListOfInt:
                    dataType = typeof(int);
                    break;
                case OperandKind.ListOfLong:
                    dataType = typeof(long);
                    break;
            }



            RqlPredicate oper;
            if (!parts.ContainsKey(target))
            {

                var op = attr.Operation;
                if (attr.Operation == RqlOperator.NotSet && dataType == typeof(string))
                    op = RqlOperator.StartsWith;
                else if (attr.Operation == RqlOperator.NotSet)
                    op = RqlOperator.Equals;

                var mapped = target;
                if (map != null && map.TryGetValue(target, out var found))
                    mapped = found;

                oper = new RqlPredicate(op, mapped, dataType, new object[] { });
                parts[target] = oper;

            }
            else
                oper = parts[target];



            switch (attr.Operand)
            {

                case OperandKind.Single:
                case OperandKind.From:
                    oper.Values.Insert(0, value);
                    break;
                case OperandKind.To:
                    oper.Values.Insert(1, value);
                    break;
                case OperandKind.List:
                    foreach (var s in (ICollection<string>)value)
                        oper.Values.Add(s.Trim());
                    break;
                case OperandKind.ListOfInt:
                    foreach (var i in (ICollection<int>)value)
                        oper.Values.Add(i);
                    break;
                case OperandKind.ListOfLong:
                    foreach (var i in (ICollection<long>)value)
                        oper.Values.Add(i);
                    break;

                default:
                    throw new RqlException($"Invalid usage. Property: {prop.Name} Operation: {attr.Operation} DataType: {prop.PropertyType.Name} Operand: {attr.Operand} - Target: {target} Value: {value}");

            }


        }


        foreach (var o in parts.Values)
            Add(o);

        return (TBuilder)this;

    }

    protected IList<IRqlPredicate> Predicates { get; }

    public bool HasCriteria => Predicates.Count > 0;
    public IEnumerable<IRqlPredicate> Criteria => Predicates;

    public int RowLimit { get; set; }

    public bool AtLeastOne( Func<IRqlPredicate, bool> predicate )
    {

        Guard.IsNotNull(predicate);

        var count = Criteria.Count(predicate);

        return count > 0;

    }

    public bool OnlyOne( Func<IRqlPredicate, bool> predicate )
    {

        Guard.IsNotNull(predicate);

        var count = Criteria.Count(predicate);

        return count == 1;

    }

    public bool None( Func<IRqlPredicate, bool> predicate)
    {

        Guard.IsNotNull(predicate);

        var count = Criteria.Count(predicate);

        return count == 0;

    }


    public void Add( IRqlPredicate predicate )
    {

        Guard.IsNotNull(predicate);

        Predicates.Add( predicate );

    }

    public void Clear()
    {
        Predicates.Clear();
    }

    #endregion


    protected string CurrentName { get; set; }



    #region Equals



    public TBuilder Equals( string value )
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add( new RqlPredicate<string>(RqlOperator.Equals, CurrentName, value) );

        return (TBuilder)this;

    }



    public TBuilder Equals(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add( new RqlPredicate<int>(RqlOperator.Equals, CurrentName, value ) );

        return (TBuilder)this;

    }


    public TBuilder Equals(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.Equals, CurrentName, value));

        return (TBuilder)this;

    }



    public TBuilder Equals(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.Equals, CurrentName, value));

        return (TBuilder)this;

    }

    public TBuilder Equals(decimal value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add( new RqlPredicate<decimal>(RqlOperator.Equals, CurrentName, value) );

        return (TBuilder)this;

    }


    public TBuilder Equals(bool value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<bool>(RqlOperator.Equals, CurrentName, value));

        return (TBuilder)this;

    }

    #endregion


    #region NotEquals

    public TBuilder NotEquals( string value )
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add( new RqlPredicate<string>(RqlOperator.NotEquals, CurrentName, value) );

        return (TBuilder)this;

    }


    public TBuilder NotEquals(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.NotEquals, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder NotEquals(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.NotEquals, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder NotEquals(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.NotEquals, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder NotEquals(decimal value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.NotEquals, CurrentName, value));

        return (TBuilder)this;

    }

    #endregion


    #region LesserThan

    public TBuilder LesserThan( string value )
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.LesserThan, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder LesserThan(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.LesserThan, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder LesserThan(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.LesserThan, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder LesserThan(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.LesserThan, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder LesserThan( decimal value )
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.LesserThan, CurrentName, value));

        return (TBuilder)this;

    }


    #endregion


    #region LesserThanOrEqual

    public TBuilder LesserThanOrEqual( string value )
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.LesserThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder LesserThanOrEqual(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.LesserThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder LesserThanOrEqual(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add( new RqlPredicate<long>(RqlOperator.LesserThanOrEqual, CurrentName, value) );

        return (TBuilder)this;

    }


    public TBuilder LesserThanOrEqual(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.LesserThanOrEqual, CurrentName, value));


        return (TBuilder)this;

    }


    public TBuilder LesserThanOrEqual(decimal value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.LesserThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }


    #endregion


    #region GreaterThan

    public TBuilder GreaterThan( string value )
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.GreaterThan, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder GreaterThan(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.GreaterThan, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder GreaterThan(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.GreaterThan, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder GreaterThan(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.GreaterThan, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder GreaterThan( decimal value )
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add( new RqlPredicate<decimal>(RqlOperator.GreaterThan, CurrentName, value) );

        return (TBuilder)this;

    }


    #endregion


    #region GreaterThanOrEqual

    public TBuilder GreaterThanOrEqual(  string value )
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.GreaterThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder GreaterThanOrEqual(int value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.GreaterThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder GreaterThanOrEqual(long value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.GreaterThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder GreaterThanOrEqual(DateTime value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.GreaterThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }


    public TBuilder GreaterThanOrEqual(decimal value)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.GreaterThanOrEqual, CurrentName, value));

        return (TBuilder)this;

    }


    #endregion


    #region String operations

    public TBuilder StartsWith( string value )
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.StartsWith, CurrentName, value));

        return (TBuilder)this;

    }

    public TBuilder Contains( string value )
    {

        Guard.IsNotNullOrWhiteSpace(value);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.Contains, CurrentName, value));

        return (TBuilder)this;

    }

    #endregion


    #region Between

    public TBuilder Between(int from, int to)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>( RqlOperator.Between, CurrentName, new []{from,to}) );


        return (TBuilder)this;

    }


    public TBuilder Between(long from, long to)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.Between, CurrentName, new[] { from, to }));

        return (TBuilder)this;

    }


    public TBuilder Between(DateTime from, DateTime to)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.Between, CurrentName, new[] { from, to }));

        return (TBuilder)this;

    }

    public TBuilder Between(decimal from, decimal to)
    {

        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.Between, CurrentName, new[] { from, to }));

        return (TBuilder)this;

    }


    public TBuilder Between(  string from,  string to )
    {

        Guard.IsNotNull(from);
        Guard.IsNotNull(to);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.Between, CurrentName, new[] { from, to }));

        return (TBuilder)this;

    }

    #endregion


    #region In

    public TBuilder In( params string[] values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder In( IEnumerable<string> values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder In( params int[] values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    public TBuilder In( IEnumerable<int> values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder In( params long[] values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder In( IEnumerable<long> values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }



    public TBuilder In( params decimal[] values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder In( IEnumerable<decimal> values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder In(  params DateTime[] values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder In(  IEnumerable<DateTime> values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.In, CurrentName, values));

        return (TBuilder)this;

    }

    #endregion


    #region NotIn


    public TBuilder NotIn( params string[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder NotIn( IEnumerable<string> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<string>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder NotIn( params int[] values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder NotIn( IEnumerable<int> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<int>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }



    public TBuilder NotIn( params long[] values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }



    public TBuilder NotIn( IEnumerable<long> values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<long>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }



    public TBuilder NotIn( params decimal[] values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }



    public TBuilder NotIn( IEnumerable<decimal> values )
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<decimal>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder NotIn( params DateTime[] values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }


    public TBuilder NotIn( IEnumerable<DateTime> values)
    {

        Guard.IsNotNull(values);
        Guard.IsNotNullOrWhiteSpace(CurrentName);

        Predicates.Add(new RqlPredicate<DateTime>(RqlOperator.NotIn, CurrentName, values));

        return (TBuilder)this;

    }

    #endregion


}
