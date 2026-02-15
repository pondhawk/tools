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

namespace Pondhawk.Rules;

/// <summary>
/// Aggregates the outcomes of a rule evaluation, including events, scores, timing, and fired rule statistics.
/// </summary>
/// <remarks>
/// <para><b>Scoring model:</b> <c>TotalAffirmations</c> and <c>TotalVetos</c> accumulate from <c>ThenAffirm(weight)</c>
/// and <c>ThenVeto(weight)</c> rule consequences. <c>Score</c> = Affirmations - Vetos.
/// Use <c>IRuleSet.Decide()</c> to compare <c>Score</c> against a threshold.</para>
/// <para><b>Events:</b> Stored as <see cref="RuleEvent"/> instances with categories: <c>Info</c>, <c>Warning</c>, <c>Violation</c>.
/// Use <c>GetViolations()</c>, <c>GetEventsByGroup()</c>, or <c>GetEventsByRule()</c> to filter.</para>
/// <para><b>Mutex winners:</b> <c>MutexWinners</c> records which rule won in each mutex group.</para>
/// <para><b>Timing:</b> <c>Started</c>/<c>Completed</c> timestamps with <c>Duration</c> in milliseconds.</para>
/// </remarks>
public sealed class EvaluationResults
{

    /// <summary>Initializes a new instance of the <see cref="EvaluationResults"/> class.</summary>
    public EvaluationResults()
    {
        Events = new HashSet<RuleEvent>();
        Shared = new Dictionary<string, object>(StringComparer.Ordinal);

        TotalEvaluated = 0;
        TotalFired = 0;

        Started = DateTime.Now;
        Completed = DateTime.MaxValue;

        FiredRules = new Dictionary<string, int>(StringComparer.Ordinal);

        MutexWinners = new Dictionary<string, string>(StringComparer.Ordinal);
    }

    /// <summary>Gets the shared dictionary for passing data between rule consequences.</summary>
    public IDictionary<string, object> Shared { get; }

    /// <summary>Gets or sets the total affirmation weight accumulated during evaluation.</summary>
    public int TotalAffirmations { get; set; }

    /// <summary>Gets or sets the total veto weight accumulated during evaluation.</summary>
    public int TotalVetos { get; set; }

    /// <summary>Gets the net score (affirmations minus vetos).</summary>
    public int Score => TotalAffirmations - TotalVetos;

    /// <summary>Gets the set of events emitted during evaluation.</summary>
    public ISet<RuleEvent> Events { get; }

    /// <summary>Gets or sets the number of violation events.</summary>
    public int ViolationCount { get; set; }

    /// <summary>Gets a value indicating whether any violations were recorded.</summary>
    public bool HasViolations => ViolationCount > 0;

    /// <summary>Gets or sets the timestamp when evaluation started.</summary>
    public DateTime Started { get; set; }

    /// <summary>Gets or sets the timestamp when evaluation completed.</summary>
    public DateTime Completed { get; set; }

    /// <summary>Gets the total evaluation duration in milliseconds.</summary>
    public long Duration => Convert.ToInt64((Completed - Started).TotalMilliseconds);

    /// <summary>Gets or sets the total number of rule conditions evaluated.</summary>
    public int TotalEvaluated { get; set; }

    /// <summary>Gets or sets the total number of rule consequences fired.</summary>
    public int TotalFired { get; set; }

    /// <summary>Gets the dictionary mapping mutex group names to the winning rule names.</summary>
    public IDictionary<string, string> MutexWinners { get; }

    /// <summary>Gets the dictionary mapping fired rule names to their fire counts.</summary>
    public IDictionary<string, int> FiredRules { get; }

    /// <summary>Adds the specified amount to the affirmation score.</summary>
    /// <param name="amount">The affirmation weight to add.</param>
    public void Affirm(int amount)
    {
        TotalAffirmations += amount;
    }

    /// <summary>Adds the specified amount to the veto score.</summary>
    /// <param name="amount">The veto weight to add.</param>
    public void Veto(int amount)
    {
        TotalVetos += amount;
    }

    /// <summary>Returns all violation events.</summary>
    /// <returns>An enumeration of violation events.</returns>
    public IEnumerable<RuleEvent> GetViolations()
    {
        return Events.Where(e => e.Category == RuleEvent.EventCategory.Violation);
    }

    /// <summary>Returns events filtered by the specified category.</summary>
    /// <param name="category">The event category to filter by.</param>
    /// <returns>An enumeration of matching events.</returns>
    public IEnumerable<RuleEvent> GetEventsByCategory(RuleEvent.EventCategory category)
    {
        return Events.Where(e => e.Category == category);
    }

    /// <summary>Returns events filtered by the specified group name.</summary>
    /// <param name="group">The group name to filter by.</param>
    /// <returns>An enumeration of matching events.</returns>
    public IEnumerable<RuleEvent> GetEventsByGroup(string group)
    {
        return Events.Where(e => string.Equals(e.Group, group, StringComparison.Ordinal));
    }

    /// <summary>Returns events filtered by the specified rule name.</summary>
    /// <param name="ruleName">The rule name to filter by.</param>
    /// <returns>An enumeration of matching events.</returns>
    public IEnumerable<RuleEvent> GetEventsByRule(string ruleName)
    {
        return Events.Where(e => string.Equals(e.RuleName, ruleName, StringComparison.Ordinal));
    }

    /// <summary>Returns violations grouped by their group name.</summary>
    /// <returns>A dictionary mapping group names to lists of violation events.</returns>
    public IDictionary<string, List<RuleEvent>> GetViolationsByGroup()
    {
        return GetViolations()
            .GroupBy(e => e.Group ?? "", StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
    }

}

