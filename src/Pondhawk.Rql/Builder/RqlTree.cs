namespace Pondhawk.Rql.Builder;

public class RqlTree
{

    public bool HasCriteria => Criteria.Count > 0;
    public List<IRqlPredicate> Criteria { get;  } = [];

}