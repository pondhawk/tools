using System.Linq.Expressions;
using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Validators;
using Pondhawk.Rules.Util;

namespace Pondhawk.Rules;

public abstract class ValidationBuilder<TFact>: AbstractRuleBuilder, IBuilder
{

    private int _currentSalience = 100000;
    private string _currentMutex = "";

    public virtual IValidator<TFact,TType> Assert<TType>( Expression<Func<TFact,TType>> extractor )
    {

        var nameSpace = GetType().Namespace;
        var fullSetName = $"{nameSpace}.{SetName}";

        var factName = typeof(TFact).GetConciseName();
        var ruleName = (extractor.Body is MemberExpression body ? $"{factName}.{body.Member.Name}" : factName);

        var rule = new ValidationRule<TFact>( fullSetName, ruleName );


        // Apply default salience
        rule.WithSalience(_currentSalience);

        // Apply default inception and expiration
        rule.WithInception(DefaultInception);
        rule.WithExpiration(DefaultExpiration);


        if (_currentPredicate is not null)
            rule.When(_currentPredicate);

        if ( !string.IsNullOrWhiteSpace(_currentMutex) )
            rule.InMutex(_currentMutex);


        Sinks.Add(t => t.Add(typeof(TFact), rule));

        _currentSalience += 100;

        var validator = rule.Assert(extractor);

        return validator;

    }


    protected ValidationRule<TFact> Rule()
    {

        var nameSpace = GetType().Namespace;
        var fullSetName = $"{nameSpace}.{SetName}";

        var factName = typeof(TFact).GetConciseName();
        var ruleName = factName;

        var rule = new ValidationRule<TFact>( fullSetName, ruleName );


        rule.WithSalience(_currentSalience);

        // Apply default inception and expiration
        rule.WithInception(DefaultInception);
        rule.WithExpiration(DefaultExpiration);


        if (_currentPredicate is not null)
            rule.When(_currentPredicate);


        if (!string.IsNullOrWhiteSpace(_currentMutex))
            rule.InMutex(_currentMutex);


        Sinks.Add(t => t.Add(typeof(TFact), rule));

        _currentSalience += 100;

        return rule;

    }


    protected void Mutex(Action builder)
    {

        try
        {
            _currentMutex = Ulid.NewUlid().ToString();
            builder();
        }
        finally
        {
            _currentMutex = "";
        }

    }


    protected void Mutex( string name, Action builder )
    {

        try
        {
            _currentMutex = name;
            builder();
        }
        finally
        {
            _currentMutex = "";
        }

    }


    private Func<TFact, bool> _currentPredicate;
    protected void When( Func<TFact,bool> predicate, Action builder )
    {

        try
        {
            _currentPredicate = predicate;
            builder();
        }
        finally
        {
            _currentPredicate = null;
        }

    }


    protected void Unless( Func<TFact,bool> predicate, Action builder )
    {
        When(f => !predicate(f), builder);
    }


    public virtual EnumerableValidator<TFact, TType> AssertOver<TType>( Expression<Func<TFact, IEnumerable<TType>>> extractor )
    {

        var nameSpace = GetType().Namespace;
        var fullSetName = $"{nameSpace}.{SetName}";

        var factName = typeof(TFact).GetConciseName();
        var ruleName = (extractor.Body is MemberExpression body ? $"{factName}.{body.Member.Name}" : factName);

        var rule = new ValidationRule<TFact>( fullSetName, ruleName );

        rule.WithSalience(_currentSalience);
        rule.WithInception(DefaultInception);
        rule.WithExpiration(DefaultExpiration);

        if (_currentPredicate is not null)
            rule.When(_currentPredicate);

        if ( !string.IsNullOrWhiteSpace(_currentMutex) )
            rule.InMutex(_currentMutex);

        Sinks.Add(t => t.Add(typeof(TFact), rule));

        _currentSalience += 100;

        return rule.AssertOver(extractor);

    }


}