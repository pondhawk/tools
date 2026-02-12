using Pondhawk.Rql.Builder;
using Pondhawk.Rql.Parser;
using Shouldly;
using Xunit;

namespace Pondhawk.Rql.Tests;

public class RqlLanguageParserTests
{

    // ========== Basic operator parsing ==========

    [Fact]
    public void ToFilter_ParsesEquals_String()
    {
        var tree = RqlLanguageParser.ToFilter("(eq(Name,'John'))");

        tree.HasCriteria.ShouldBeTrue();
        tree.Criteria.Count.ShouldBe(1);

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Name");
        pred.DataType.ShouldBe(typeof(string));
        pred.Values[0].ShouldBe("John");
    }

    [Fact]
    public void ToFilter_ParsesEquals_Int()
    {
        var tree = RqlLanguageParser.ToFilter("(eq(Quantity,42))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.Target.Name.ShouldBe("Quantity");
        pred.DataType.ShouldBe(typeof(int));
        pred.Values[0].ShouldBe(42);
    }

    [Fact]
    public void ToFilter_ParsesEquals_Long()
    {
        var tree = RqlLanguageParser.ToFilter("(eq(Sku,9876543210))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.DataType.ShouldBe(typeof(long));
        pred.Values[0].ShouldBe(9876543210L);
    }

    [Fact]
    public void ToFilter_ParsesEquals_Decimal()
    {
        var tree = RqlLanguageParser.ToFilter("(eq(Price,#19.99))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.DataType.ShouldBe(typeof(decimal));
        pred.Values[0].ShouldBe(19.99m);
    }

    [Fact]
    public void ToFilter_ParsesEquals_DateTime()
    {
        var tree = RqlLanguageParser.ToFilter("(eq(Created,@2024-01-15T00:00:00Z))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.DataType.ShouldBe(typeof(DateTime));

        var dt = (DateTime)pred.Values[0];
        dt.Year.ShouldBe(2024);
        dt.Month.ShouldBe(1);
        dt.Day.ShouldBe(15);
    }

    [Fact]
    public void ToFilter_ParsesEquals_Bool()
    {
        var tree = RqlLanguageParser.ToFilter("(eq(IsActive,true))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Equals);
        pred.DataType.ShouldBe(typeof(bool));
        pred.Values[0].ShouldBe(true);
    }


    // ========== All operator codes ==========

    [Fact]
    public void ToFilter_ParsesNotEquals()
    {
        var tree = RqlLanguageParser.ToFilter("(ne(Name,'John'))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.NotEquals);
    }

    [Fact]
    public void ToFilter_ParsesLesserThan()
    {
        var tree = RqlLanguageParser.ToFilter("(lt(Quantity,10))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.LesserThan);
    }

    [Fact]
    public void ToFilter_ParsesGreaterThan()
    {
        var tree = RqlLanguageParser.ToFilter("(gt(Quantity,10))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.GreaterThan);
    }

    [Fact]
    public void ToFilter_ParsesLesserThanOrEqual()
    {
        var tree = RqlLanguageParser.ToFilter("(le(Quantity,10))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.LesserThanOrEqual);
    }

    [Fact]
    public void ToFilter_ParsesGreaterThanOrEqual()
    {
        var tree = RqlLanguageParser.ToFilter("(ge(Quantity,10))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.GreaterThanOrEqual);
    }

    [Fact]
    public void ToFilter_ParsesStartsWith()
    {
        var tree = RqlLanguageParser.ToFilter("(sw(Name,'Wid'))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.StartsWith);
        tree.Criteria[0].Values[0].ShouldBe("Wid");
    }

    [Fact]
    public void ToFilter_ParsesContains()
    {
        var tree = RqlLanguageParser.ToFilter("(cn(Name,'idg'))");

        tree.Criteria[0].Operator.ShouldBe(RqlOperator.Contains);
        tree.Criteria[0].Values[0].ShouldBe("idg");
    }


    // ========== Multi-value operations ==========

    [Fact]
    public void ToFilter_ParsesBetween_Decimal()
    {
        var tree = RqlLanguageParser.ToFilter("(bt(Price,#10.00,#50.00))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.Between);
        pred.DataType.ShouldBe(typeof(decimal));
        pred.Values.Count.ShouldBe(2);
        pred.Values[0].ShouldBe(10.00m);
        pred.Values[1].ShouldBe(50.00m);
    }

    [Fact]
    public void ToFilter_ParsesIn_Strings()
    {
        var tree = RqlLanguageParser.ToFilter("(in(Status,'Active','Pending'))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.In);
        pred.DataType.ShouldBe(typeof(string));
        pred.Values.Count.ShouldBe(2);
        pred.Values[0].ShouldBe("Active");
        pred.Values[1].ShouldBe("Pending");
    }

    [Fact]
    public void ToFilter_ParsesNotIn_Ints()
    {
        var tree = RqlLanguageParser.ToFilter("(ni(Quantity,1,2,3))");

        var pred = tree.Criteria[0];
        pred.Operator.ShouldBe(RqlOperator.NotIn);
        pred.DataType.ShouldBe(typeof(int));
        pred.Values.Count.ShouldBe(3);
    }


    // ========== Multiple predicates ==========

    [Fact]
    public void ToFilter_ParsesMultiplePredicates()
    {
        var tree = RqlLanguageParser.ToFilter("(eq(Name,'Widget'),gt(Quantity,10))");

        tree.Criteria.Count.ShouldBe(2);
        tree.Criteria[0].Operator.ShouldBe(RqlOperator.Equals);
        tree.Criteria[0].Target.Name.ShouldBe("Name");
        tree.Criteria[1].Operator.ShouldBe(RqlOperator.GreaterThan);
        tree.Criteria[1].Target.Name.ShouldBe("Quantity");
    }


    // ========== Edge cases ==========

    [Fact]
    public void ToFilter_EmptyCriteria_ReturnsEmptyTree()
    {
        var tree = RqlLanguageParser.ToFilter("()");

        tree.HasCriteria.ShouldBeFalse();
        tree.Criteria.Count.ShouldBe(0);
    }

    [Fact]
    public void ToFilter_InvalidInput_ThrowsRqlException()
    {
        Should.Throw<RqlException>(() => RqlLanguageParser.ToFilter("invalid"));
    }

    [Fact]
    public void ToCriteria_ParsesSameAsToFilter()
    {
        var fromFilter = RqlLanguageParser.ToFilter("(eq(Name,'John'),gt(Quantity,10))");
        var fromCriteria = RqlLanguageParser.ToCriteria("(eq(Name,'John'),gt(Quantity,10))");

        fromFilter.Criteria.Count.ShouldBe(fromCriteria.Criteria.Count);
        fromFilter.Criteria[0].Operator.ShouldBe(fromCriteria.Criteria[0].Operator);
        fromFilter.Criteria[0].Target.Name.ShouldBe(fromCriteria.Criteria[0].Target.Name);
        fromFilter.Criteria[1].Operator.ShouldBe(fromCriteria.Criteria[1].Operator);
    }

}
