namespace Pondhawk.Rql.Builder
{


    public class RqlPredicate: RqlPredicate<object>
    {


        public RqlPredicate( RqlOperator op,  string name,  Type dataType,  object value ): base( op, name, value )
        {
            DataType = dataType;
        }


        public RqlPredicate( RqlOperator op,  string name,  Type dataType, IEnumerable<object> values): base( op, name, values )
        {
            DataType = dataType;
        }


    }


    public class RqlPredicate<TType>: IRqlPredicate
    {

        public RqlPredicate(RqlOperator op,  string name,  TType value )
        {

            ArgumentNullException.ThrowIfNull(value);
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            Operator = op;
            Target   = new Target(name);

            Values = new List<TType>();

            DataType = typeof(TType);
            Value    = value;

        }


        public RqlPredicate( RqlOperator op,  string name,  IEnumerable<TType> values )
        {

            ArgumentNullException.ThrowIfNull(values);
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            Operator = op;
            Target   = new Target(name);

            Values = new List<TType>();

            DataType = typeof(TType);
            Values   = new List<TType>(values);

        }


        public RqlOperator Operator { get; set; }

        public Target Target { get; set; }

        public Type DataType { get; set; }

        public IList<TType> Values { get; }

        IReadOnlyList<object> IRqlPredicate.Values => Values.Cast<object>().ToList();

        public TType Value
        {
            get => (Values.Count > 0 ? Values[0] : default!);

            set
            {
                Values.Clear();
                Values.Add(value);
            }

        }


    }



}