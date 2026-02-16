using Pondhawk.Mediator;
using Shouldly;
using Xunit;

namespace Pondhawk.Core.Tests.Mediator;

public class BatchCommandResultTests
{

    // ── Test doubles ──

    public class OrderResponse
    {
        public int OrderId { get; init; }
    }

    public class InvoiceResponse
    {
        public string InvoiceNumber { get; init; }
    }

    // ── Succeeded ──

    [Fact]
    public void Succeeded_SetsSuccessTrue()
    {
        var result = BatchCommandResult.Succeeded(new OrderResponse { OrderId = 1 });

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public void Succeeded_StoresResponse()
    {
        var response = new OrderResponse { OrderId = 42 };

        var result = BatchCommandResult.Succeeded(response);

        result.Response.ShouldBeSameAs(response);
    }

    [Fact]
    public void Succeeded_ExtractsCommandTypeFromTypeName()
    {
        var result = BatchCommandResult.Succeeded(new OrderResponse());

        result.CommandType.ShouldBe("Order");
    }

    [Fact]
    public void Succeeded_TypeNameWithoutResponse_KeepsFullName()
    {
        var result = BatchCommandResult.Succeeded("plain string");

        result.CommandType.ShouldBe("String");
    }

    [Fact]
    public void Succeeded_WithEntityUid_SetsEntityUid()
    {
        var result = BatchCommandResult.Succeeded(new OrderResponse(), "order-123");

        result.EntityUid.ShouldBe("order-123");
    }

    [Fact]
    public void Succeeded_WithoutEntityUid_EntityUidIsNull()
    {
        var result = BatchCommandResult.Succeeded(new OrderResponse());

        result.EntityUid.ShouldBeNull();
    }

    [Fact]
    public void Succeeded_ErrorMessageIsNull()
    {
        var result = BatchCommandResult.Succeeded(new OrderResponse());

        result.ErrorMessage.ShouldBeNull();
    }

    // ── Failed ──

    [Fact]
    public void Failed_SetsSuccessFalse()
    {
        var result = BatchCommandResult.Failed("CreateOrder", "order-1", "Validation failed");

        result.Success.ShouldBeFalse();
    }

    [Fact]
    public void Failed_SetsCommandType()
    {
        var result = BatchCommandResult.Failed("CreateOrder", "order-1", "Error");

        result.CommandType.ShouldBe("CreateOrder");
    }

    [Fact]
    public void Failed_SetsEntityUid()
    {
        var result = BatchCommandResult.Failed("CreateOrder", "order-1", "Error");

        result.EntityUid.ShouldBe("order-1");
    }

    [Fact]
    public void Failed_NullEntityUid_Allowed()
    {
        var result = BatchCommandResult.Failed("CreateOrder", null, "Error");

        result.EntityUid.ShouldBeNull();
    }

    [Fact]
    public void Failed_SetsErrorMessage()
    {
        var result = BatchCommandResult.Failed("CreateOrder", null, "Something broke");

        result.ErrorMessage.ShouldBe("Something broke");
    }

    [Fact]
    public void Failed_ResponseIsNull()
    {
        var result = BatchCommandResult.Failed("CreateOrder", null, "Error");

        result.Response.ShouldBeNull();
    }

    [Fact]
    public void Failed_NullCommandType_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => BatchCommandResult.Failed(null, null, "Error"));
    }

    [Fact]
    public void Failed_EmptyCommandType_Throws()
    {
        Should.Throw<ArgumentException>(
            () => BatchCommandResult.Failed("", null, "Error"));
    }

    [Fact]
    public void Failed_NullError_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => BatchCommandResult.Failed("CreateOrder", null, null));
    }

    [Fact]
    public void Failed_EmptyError_Throws()
    {
        Should.Throw<ArgumentException>(
            () => BatchCommandResult.Failed("CreateOrder", null, ""));
    }

    // ── GetResponse ──

    [Fact]
    public void GetResponse_CorrectType_ReturnsTypedResponse()
    {
        var response = new OrderResponse { OrderId = 42 };
        var result = BatchCommandResult.Succeeded(response);

        var typed = result.GetResponse<OrderResponse>();

        typed.ShouldNotBeNull();
        typed.OrderId.ShouldBe(42);
    }

    [Fact]
    public void GetResponse_WrongType_ReturnsNull()
    {
        var result = BatchCommandResult.Succeeded(new OrderResponse());

        var typed = result.GetResponse<InvoiceResponse>();

        typed.ShouldBeNull();
    }

    [Fact]
    public void GetResponse_NullResponse_ReturnsNull()
    {
        var result = BatchCommandResult.Failed("Test", null, "Error");

        var typed = result.GetResponse<OrderResponse>();

        typed.ShouldBeNull();
    }

    // ── Record equality ──

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new BatchCommandResult { Success = true, CommandType = "Test", EntityUid = "1" };
        var b = new BatchCommandResult { Success = true, CommandType = "Test", EntityUid = "1" };

        a.ShouldBe(b);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var a = new BatchCommandResult { Success = true, CommandType = "Test" };
        var b = new BatchCommandResult { Success = false, CommandType = "Test" };

        a.ShouldNotBe(b);
    }

}
