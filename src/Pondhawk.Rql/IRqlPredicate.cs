using Pondhawk.Rql.Builder;

namespace Pondhawk.Rql
{
 
    
    public interface IRqlPredicate
    {

        RqlOperator Operator { get; }

        Target Target { get; }

        Type DataType { get; }

        IReadOnlyList<object> Values { get; }

    }



}
