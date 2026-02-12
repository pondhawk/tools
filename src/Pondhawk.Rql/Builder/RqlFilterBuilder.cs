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

using System.Linq.Expressions;

namespace Pondhawk.Rql.Builder
{


    public class RqlFilterBuilder<TTarget>: AbstractFilterBuilder<RqlFilterBuilder<TTarget>>, IRqlFilter<TTarget> where TTarget: class
    {

        public static RqlFilterBuilder<TTarget> Create()
        {
            return new RqlFilterBuilder<TTarget>();
        }


        public static RqlFilterBuilder<TTarget> Where<TValue>( Expression<Func<TTarget, TValue>> prop )
        {
            var builder = new RqlFilterBuilder<TTarget>().And(prop);
            return builder;
        }

        public static RqlFilterBuilder<TTarget> All()
        {
            var builder = new RqlFilterBuilder<TTarget>();
            return builder;
        }



        protected RqlFilterBuilder()
        {
            
        }

        public RqlFilterBuilder( RqlTree tree ) : base( tree )
        {
            
        }


        public RqlFilterBuilder<TTarget> And<TValue>( Expression<Func<TTarget, TValue>> prop )
        {

            ArgumentNullException.ThrowIfNull(prop);
            ArgumentNullException.ThrowIfNull(prop.Body);

            if (!(prop.Body is MemberExpression))
                throw new ArgumentException("Targets of a builder must be a field or a property on the output model");

            var propExpr = (MemberExpression)prop.Body;

            CurrentName = propExpr.Member.Name;

            return this;

        }


        public override Type Target => typeof(TTarget);



    }


    public class RqlFilterBuilder : AbstractFilterBuilder<RqlFilterBuilder>
    {


        public static RqlFilterBuilder Create()
        {
            return new RqlFilterBuilder();
        }


        public static RqlFilterBuilder All()
        {
            var builder = new RqlFilterBuilder();
            return builder;
        }




        public static RqlFilterBuilder Where( string prop )
        {

            if (string.IsNullOrWhiteSpace(prop))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(prop));

            var builder = new RqlFilterBuilder().And(prop);
            return builder;
        }


        protected RqlFilterBuilder()
        {

        }

        public RqlFilterBuilder( RqlTree tree) : base(tree)
        {

        }


        public override Type Target => typeof(RqlFilterBuilder);

        public bool IsAll => !HasCriteria;


        public RqlFilterBuilder And( string prop )
        {

            ArgumentException.ThrowIfNullOrWhiteSpace(prop);

            CurrentName = prop;

            return this;

        }


    }


}
