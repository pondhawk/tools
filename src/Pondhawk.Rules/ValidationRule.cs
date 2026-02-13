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

using System.Linq.Expressions;
using CommunityToolkit.Diagnostics;
using Pondhawk.Rules.Builder;
using Pondhawk.Rules.Evaluation;
using Pondhawk.Rules.Validators;
using Pondhawk.Utilities.Types;

namespace Pondhawk.Rules;

public interface IValidationRule<out TFact>: IRule
{
        
}


public class ValidationRule<TFact> : AbstractRule, IValidationRule<TFact>
{

    public ValidationRule( string setName, string ruleName ) : base( setName, ruleName )
    {
        Salience = int.MaxValue - 1000000;

        OnlyFiresOnce = true;

        Predicates = [];
    }


    protected List<Func<TFact, bool>> Predicates { get; private set; }
    protected BaseValidator<TFact> TypeValidator { get; private set; }

    private Action<TFact> CascadeAction { get; set; }


        
    public ValidationRule<TFact> WithSalience( int value )
    {
        Salience = (int.MaxValue - 1000000) + value;
        return this;
    }

        
    public ValidationRule<TFact> InMutex( string name )
    {
        Guard.IsNotNullOrWhiteSpace(name);

        Mutex = name;
        return this;
    }


        
    public ValidationRule<TFact> WithInception( DateTime inception )
    {
        Inception = inception;
        return this;
    }


        
    public ValidationRule<TFact> WithExpiration( DateTime expiration )
    {
        Expiration = expiration;
        return this;
    }

        
    public ValidationRule<TFact> When( Func<TFact, bool> predicate )
    {
        Guard.IsNotNull(predicate);

        Predicates = [predicate];
        return this;
    }


        
    public ValidationRule<TFact> And( Func<TFact, bool> predicate )
    {
        Guard.IsNotNull(predicate);

        Predicates.Add(predicate);
        return this;
    }



        
    public IValidator<TFact, TType> Assert<TType>( Expression<Func<TFact, TType>> extractorEx )
    {
        Guard.IsNotNull(extractorEx);

        var factName = typeof(TFact).GetConciseName();

        var propName = "";
        var groupName = factName;
        if( extractorEx.Body is MemberExpression body )
        {
            propName = body.Member.Name;
            groupName = $"{factName}.{body.Member.Name}";
            
        }

        var extractor = extractorEx.Compile();

        var validator = new Validator<TFact, TType>( this, groupName, propName, extractor );
        TypeValidator = validator;

        return validator;

    }


    public EnumerableValidator<TFact, TType> AssertOver<TType>( Expression<Func<TFact, IEnumerable<TType>>> extractorEx )
    {
        Guard.IsNotNull(extractorEx);

        var factName = typeof(TFact).Name;

        var propName = "";
        var groupName = factName;
        if( extractorEx.Body is MemberExpression body )
        {
            propName = body.Member.Name;
            groupName = $"{factName}.{body.Member.Name}";
            
        }


        var extractor = extractorEx.Compile();

        var validator = new EnumerableValidator<TFact, TType>( this, groupName, propName, extractor );
        TypeValidator = validator;

        return validator;

    }


    public void Cascade<TRef>(  Func<TFact, TRef> extractor ) where TRef : class
    {
        Guard.IsNotNull(extractor);

        CascadeAction = f => RuleThreadLocalStorage.CurrentContext.InsertFact( extractor( f ) );
    }


    public void CascadeAll<TChild>(  Func<TFact, IEnumerable<TChild>> extractor ) where TChild : class
    {
        Guard.IsNotNull(extractor);

        CascadeAction = f => _CascadeCollection( extractor( f ) );
    }

    private void _CascadeCollection(  IEnumerable<object> children )
    {
        foreach( var o in children )
            RuleThreadLocalStorage.CurrentContext.InsertFact( o );
    }


    protected override IRule InternalEvaluate(  object[] offered  )
    {
        base.InternalEvaluate( offered );

        var fact = (TFact)offered[0];

        if( CascadeAction is not null )
            return this;

        if( (Predicates.Count > 0) && (Predicates.Select( cond=>cond(fact) ).Any( r=>r == false )) )
            return null;

        if( TypeValidator.Conditions.Select( cond => cond( fact ) ).Any( result => !result ) )
            return this;

        return null;

    }


    protected override void InternalFire(  object[] offered )
    {
        base.InternalFire( offered );

        var fact = (TFact)offered[0];

        if( CascadeAction is not null )
            CascadeAction( fact );
        else
            TypeValidator.Consequence( fact );
    }
}