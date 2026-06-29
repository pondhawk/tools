using Pondhawk.Exceptions;
using Pondhawk.Mediator;
using Shouldly;
using Xunit;

namespace Pondhawk.Core.Tests.Mediator;

public class ResponseTests
{

    [Fact]
    public void ImplicitConversion_FromValue_ProducesSuccess()
    {
        Response<int> response = 42;

        response.Ok.ShouldBeTrue();
        response.Value.ShouldBe(42);
        response.Error.ShouldBeNull();
    }

    [Fact]
    public void ImplicitConversion_IsEquivalentToSuccess()
    {
        Response<string> implicitly = "hello";
        var explicitly = Response<string>.Success("hello");

        implicitly.ShouldBe(explicitly);
    }

    [Fact]
    public void Failure_SetsErrorAndNotOk()
    {
        var error = new ErrorInfo { Kind = ErrorKind.NotFound, ErrorCode = "NotFound", Explanation = "gone" };

        var response = Response<string>.Failure(error);

        response.Ok.ShouldBeFalse();
        response.Value.ShouldBeNull();
        response.Error.ShouldBeSameAs(error);
    }

    [Fact]
    public void Match_RoutesByOutcome()
    {
        Response<int> ok = 7;
        var failed = Response<int>.Failure(new ErrorInfo { Kind = ErrorKind.System, ErrorCode = "System", Explanation = "boom" });

        ok.Match(v => $"ok:{v}", e => $"err:{e.Kind}").ShouldBe("ok:7");
        failed.Match(v => $"ok:{v}", e => $"err:{e.Kind}").ShouldBe("err:System");
    }

    [Fact]
    public void GetValueOrThrow_OnFailure_Throws()
    {
        var failed = Response<int>.Failure(new ErrorInfo { Kind = ErrorKind.Conflict, ErrorCode = "Conflict", Explanation = "nope" });

        Should.Throw<InvalidOperationException>(() => failed.GetValueOrThrow());
    }

}
