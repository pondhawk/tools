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

using CommunityToolkit.Diagnostics;
using Pondhawk.Rules.Builder;
using Pondhawk.Utilities.Types;
using TypeExtensions = Pondhawk.Utilities.Types.TypeExtensions;

namespace Pondhawk.Rules;

public abstract class RuleBuilder : AbstractRuleBuilder, IBuilder
{

        
    public virtual Rule<TFact> Rule<TFact>( params string[] tags )
    {

        var nameSpace = GetType().Namespace;
        var fullSetName = $"{nameSpace}.{SetName}";
        var tagsSegment = tags.Length > 0 ? $":{string.Join(':', tags)}" : "";
        var ruleName = $"({typeof(TFact).GetConciseName()}){tagsSegment}";

        var rule = new Rule<TFact>(fullSetName, ruleName);

        // Apply the default for FireOnce
        if (DefaultFireOnce)
            rule.FireOnce();
        else
            rule.FireAlways();


        // Apply default salience
        rule.WithSalience(DefaultSalience);

        // Apply default inception and expiration
        rule.WithInception(DefaultInception);
        rule.WithExpiration(DefaultExpiration);


        Sinks.Add( t => t.Add( typeof(TFact), rule ) );

        return rule;

    }


        
    public virtual ForeachRule<TFact, TChild> Rule<TFact,TChild>( Func<TFact, IEnumerable<TChild>> extractor, params string[] tags )
    {


        var nameSpace = GetType().Namespace;
        var fullSetName = $"{nameSpace}.{SetName}";
        var tagsSegment = tags.Length > 0 ? $":{string.Join(':', tags)}" : "";
        var ruleName = $"({typeof(TFact).GetConciseName()}[{TypeExtensions.GetConciseName(typeof(TChild))}]){tagsSegment}";


        var rule = new ForeachRule<TFact, TChild>(extractor, fullSetName, ruleName);

        // Apply default salience
        rule.WithSalience(DefaultSalience);

        // Apply default inception and expiration
        rule.WithInception(DefaultInception);
        rule.WithExpiration(DefaultExpiration);

        Sinks.Add(t => t.Add(typeof(TFact), rule));

        return rule;

    }


}


public abstract class RuleBuilder<TFact> : AbstractRuleBuilder, IBuilder
{


    protected RuleBuilder()
    {
        Targets = [typeof(TFact)];
    }


    public virtual Rule<TFact> Rule( params string[] tags)
    {

        var nameSpace = GetType().Namespace;
        var fullSetName = $"{nameSpace}.{SetName}";
        var tagsSegment = tags.Length > 0 ? $":{string.Join(':', tags)}" : "";
        var ruleName = $"({typeof(TFact).GetConciseName()}){tagsSegment}";

        var rule = new Rule<TFact>( fullSetName, ruleName );

        // Apply the default for FireOnce
        if (DefaultFireOnce)
            rule.FireOnce();
        else
            rule.FireAlways();


        // Apply default salience
        rule.WithSalience( DefaultSalience );

        // Apply default inception and expiration
        rule.WithInception( DefaultInception );
        rule.WithExpiration( DefaultExpiration );

        Rules.Add( rule );

        return rule;
    }


        
    public virtual ForeachRule<TFact, TChild> Rule<TChild>( Func<TFact, IEnumerable<TChild>> extractor, params string[] tags )
    {


        var nameSpace = GetType().Namespace;
        var fullSetName = $"{nameSpace}.{SetName}";
        var tagsSegment = tags.Length > 0 ? $":{string.Join(':', tags)}" : "";
        var ruleName = $"({typeof(TFact).GetConciseName()}[{TypeExtensions.GetConciseName(typeof(TChild))}]){tagsSegment}";


        var rule = new ForeachRule<TFact, TChild>( extractor, fullSetName, ruleName );

        // Apply default salience
        rule.WithSalience( DefaultSalience );

        // Apply default inception and expiration
        rule.WithInception( DefaultInception );
        rule.WithExpiration( DefaultExpiration );

        Rules.Add( rule );

        return rule;

    }


}



/// <summary>
/// Responsible for the creation and collection of rules that reason over a two
/// facts. The two fact RuleBuilder allows for the creation and collection of rules.
/// At runtime RuleBuilders are discovered in supplied assemblies and initialied to
/// create and collect into the a RuleBase. The RuleBase then serves up the rules
/// for evaluation.
/// </summary>
/// <typeparam name="TFact1">The first Type of fact that this rule reasons
/// over</typeparam>
/// <typeparam name="TFact2">The second Type of fact that this rule reasons
/// over</typeparam>
public abstract class RuleBuilder<TFact1, TFact2> : AbstractRuleBuilder, IBuilder
{


    protected RuleBuilder()
    {
        Targets = [typeof(TFact1), typeof(TFact2)];
    }

    /// <summary>
    /// Adds a rule that reasons over the two fact types defined for this builder
    /// </summary>
    /// <param name="ruleName">The name for the rule. This is required and should be
    /// unique within the builder where the rule is defined. I can be anything you like
    /// and serves no operational function. However it is very useful when you are
    /// troubleshoot your rules and is used in the EvaluationResults statistics.</param>
    /// <returns>
    /// The newly created rule for the two given fact types of this builder
    /// </returns>
        
    public virtual Rule<TFact1, TFact2> AddRule( string ruleName )
    {
        Guard.IsNotNullOrEmpty(ruleName);


        var nameSpace = GetType().Namespace;
        var fqSetName = $"{nameSpace}.{SetName}";

        var rule = new Rule<TFact1, TFact2>( fqSetName, ruleName );

        // Apply the default for FireOnce
        if (DefaultFireOnce)
            rule.FireOnce();
        else
            rule.FireAlways();

        // Apply default salience
        rule.WithSalience( DefaultSalience );

        // Apply default inception and expiration
        rule.WithInception( DefaultInception );
        rule.WithExpiration( DefaultExpiration );

        Rules.Add( rule );

        return rule;
    }

}


/// <summary>
/// Responsible for the creation and collection of rules that reason over three
/// facts. The three fact RuleBuilder allows for the creation and collection of rules.
/// At runtime RuleBuilders are discovered in supplied assemblies and initialied to
/// create and collect into the a RuleBase. The RuleBase then serves up the rules
/// for evaluation.
/// </summary>
/// <typeparam name="TFact1">The first Type of fact that this rule reasons
/// over</typeparam>
/// <typeparam name="TFact2">The second Type of fact that this rule reasons
/// over</typeparam>
/// <typeparam name="TFact3">The third Type of fact that this rule reasons
/// over</typeparam>
public abstract class RuleBuilder<TFact1, TFact2, TFact3> : AbstractRuleBuilder, IBuilder
{

    protected RuleBuilder()
    {
        Targets = [typeof(TFact1), typeof(TFact2), typeof(TFact3)];
    }
        

    /// <summary>
    /// Adds a rule that reasons over the three fact types defined for this builder
    /// </summary>
    /// <param name="ruleName">The name for the rule. This is required and should be
    /// unique within the builder where the rule is defined. I can be anything you like
    /// and serves no operational function. However it is very useful when you are
    /// troubleshoot your rules and is used in the EvaluationResults statistics.</param>
    /// <returns>
    /// The newly created rule for the three given fact types of this builder
    /// </returns>
        
    public virtual Rule<TFact1, TFact2, TFact3> AddRule( string ruleName )
    {
        Guard.IsNotNullOrEmpty(ruleName);


        var nameSpace = GetType().Namespace;
        var fqSetName = $"{nameSpace}.{SetName}";

        var rule = new Rule<TFact1, TFact2, TFact3>( fqSetName, ruleName );

        // Apply the default for FireOnce
        if( DefaultFireOnce )
            rule.FireOnce();
        else
            rule.FireAlways();


        // Apply default salience
        rule.WithSalience( DefaultSalience );

        // Apply default inception and expiration
        rule.WithInception( DefaultInception );
        rule.WithExpiration( DefaultExpiration );

        Rules.Add( rule );

        return rule;
    }
}


/// <summary>
/// Responsible for the creation and collection of rules that reason over four
/// facts. The four fact RuleBuilder allows for the creation and collection of rules.
/// At runtime RuleBuilders are discovered in supplied assemblies and initialied to
/// create and collect into the a RuleBase. The RuleBase then serves up the rules
/// for evaluation.
/// </summary>
/// <typeparam name="TFact1">The first Type of fact that this rule reasons
/// over</typeparam>
/// <typeparam name="TFact2">The second Type of fact that this rule reasons
/// over</typeparam>
/// <typeparam name="TFact3">The third Type of fact that this rule reasons
/// over</typeparam>
/// <typeparam name="TFact4">The fourth Type of fact that this rule reasons
/// over</typeparam>
public abstract class RuleBuilder<TFact1, TFact2, TFact3, TFact4> : AbstractRuleBuilder, IBuilder
{


    protected RuleBuilder()
    {
        Targets = [typeof(TFact1), typeof(TFact2), typeof(TFact3), typeof(TFact4)];
    }



    /// <summary>
    /// Adds a rule that reasons over the four fact types defined for this builder
    /// </summary>
    /// <param name="ruleName">The name for the rule. This is required and should be
    /// unique within the builder where the rule is defined. I can be anything you like
    /// and serves no operational function. However it is very useful when you are
    /// troubleshoot your rules and is used in the EvaluationResults statistics.</param>
    /// <returns>
    /// The newly created rule for the four given fact types of this builder
    /// </returns>
        
    public virtual Rule<TFact1, TFact2, TFact3, TFact4> AddRule( string ruleName )
    {
        Guard.IsNotNullOrEmpty(ruleName);


        var nameSpace = GetType().Namespace;
        var fqSetName = $"{nameSpace}.{SetName}";

        var rule = new Rule<TFact1, TFact2, TFact3, TFact4>( fqSetName, ruleName );

        // Apply the default for FireOnce
        if( DefaultFireOnce )
            rule.FireOnce();
        else
            rule.FireAlways();


        // Apply default salience
        rule.WithSalience( DefaultSalience );

        // Apply default inception and expiration
        rule.WithInception( DefaultInception );
        rule.WithExpiration( DefaultExpiration );

        Rules.Add( rule );

        return rule;
    }
    
}