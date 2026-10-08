using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Application.Tests.Sales.Commands.CheckoutSale;

public class CheckoutSaleCommandValidatorTests
{
    private readonly CheckoutSaleCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldAcceptValidCheckout()
    {
        var command = new CheckoutSaleCommand(
            Guid.NewGuid(),
            "checkout-001",
            new[]
            {
                new CheckoutPaymentRequest(
                    PaymentMethod.Cash,
                    100,
                    "payment-001")
            });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldRejectEmptyCheckoutIdempotencyKey()
    {
        var command = new CheckoutSaleCommand(
            Guid.NewGuid(),
            "",
            Array.Empty<CheckoutPaymentRequest>());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldRejectNonPositivePayment()
    {
        var command = new CheckoutSaleCommand(
            Guid.NewGuid(),
            "checkout-002",
            new[]
            {
                new CheckoutPaymentRequest(
                    PaymentMethod.Cash,
                    0,
                    "payment-002")
            });

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
