namespace Pondhawk.Rql.Builder;

public class RqlTree
{

    public bool HasProjection => Projection.Count > 0;
    public List<string> Projection { get; } = [];

    public bool HasCriteria => Criteria.Count > 0;
    public List<IRqlPredicate> Criteria { get;  } = [];

}