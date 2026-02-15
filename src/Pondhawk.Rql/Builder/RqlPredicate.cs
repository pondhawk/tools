using CommunityToolkit.Diagnostics;

namespace Pondhawk.Rql.Builder
{


    /// <summary>
    /// An untyped RQL predicate with an explicit <see cref="IRqlPredicate.DataType"/>.
    /// Used by the parser when the value type is determined at parse time.
    /// </summary>
    public class RqlPredicate : RqlPredicate<object>
    {


        public RqlPredicate(RqlOperator op, string name, Type dataType, object value) : base(op, name, value)
        {
            DataType = dataType;
        }


        public RqlPredicate(RqlOperator op, string name, Type dataType, IEnumerable<object> values) : base(op, name, values)
        {
            DataType = dataType;
        }


    }


    /// <summary>
    /// A strongly-typed RQL predicate holding values of type <typeparamref name="TType"/>.
    /// </summary>
    public class RqlPredicate<TType> : IRqlPredicate
    {

        private IReadOnlyList<object>? _cachedValues;

        public RqlPredicate(RqlOperator op, string name, TType value)
        {

            Guard.IsNotNull(value);
            Guard.IsNotNullOrWhiteSpace(name);

            Operator = op;
            Target = new Target(name);

            Values = new List<TType>();

            DataType = typeof(TType);
            Value = value;

        }


        public RqlPredicate(RqlOperator op, string name, IEnumerable<TType> values)
        {

            Guard.IsNotNull(values);
            Guard.IsNotNullOrWhiteSpace(name);

            Operator = op;
            Target = new Target(name);

            DataType = typeof(TType);
            Values = new List<TType>(values);

        }


        public RqlOperator Operator { get; set; }

        public Target Target { get; set; }

        public Type DataType { get; set; }

        public IList<TType> Values { get; }

        IReadOnlyList<object> IRqlPredicate.Values => _cachedValues ??= Values.Cast<object>().ToList();

        public TType Value
        {
            get => (Values.Count > 0 ? Values[0] : default!);

            set
            {
                _cachedValues = null;
                Values.Clear();
                Values.Add(value);
            }

        }


    }



}
