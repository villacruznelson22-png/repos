using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.Entities;

public class PaymentTests
{
    [Fact]
    public void Constructor_WithValidValues_CreatesPayment()
    {
        var saleId = Guid.NewGuid();
        var paidAt = DateTime.UtcNow;
        var payment = new Payment(
            saleId,
            PaymentMethod.GCash,
            new Money(1500.00m),
            paidAt,
            " payment-key-001 ",
            " GC-12345 ");

        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(saleId, payment.SaleId);
        Assert.Equal(PaymentMethod.GCash, payment.Method);
        Assert.Equal(new Money(1500.00m), payment.Amount);
        Assert.Equal(paidAt, payment.PaidAt);
        Assert.Equal("payment-key-001", payment.IdempotencyKey);
        Assert.Equal("GC-12345", payment.ReferenceNumber);
    }

    [Fact]
    public void Constructor_WithEmptySaleId_ThrowsSaleDomainException()
    {
        var exception = Assert.Throws<SaleDomainException>(() =>
            new Payment(
                Guid.Empty,
                PaymentMethod.Cash,
                new Money(100),
                DateTime.UtcNow,
                "KEY-001"));

        Assert.Equal("Sale ID cannot be empty.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositiveAmount_ThrowsSaleDomainException(
        decimal amount)
    {
        var exception = Assert.Throws<SaleDomainException>(() =>
            new Payment(
                Guid.NewGuid(),
                PaymentMethod.Cash,
                new Money(amount),
                DateTime.UtcNow,
                "KEY-001"));

        Assert.Equal(
            "Payment amount must be greater than zero.",
            exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithEmptyIdempotencyKey_ThrowsSaleDomainException(
        string idempotencyKey)
    {
        var exception = Assert.Throws<SaleDomainException>(() =>
            new Payment(
                Guid.NewGuid(),
                PaymentMethod.Cash,
                new Money(100),
                DateTime.UtcNow,
                idempotencyKey));

        Assert.Equal(
            "Payment idempotency key cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithBlankReferenceNumber_SetsReferenceNumberToNull()
    {
        var payment = new Payment(
            Guid.NewGuid(),
            PaymentMethod.Cash,
            new Money(100),
            DateTime.UtcNow,
            "KEY-001",
            "   ");

        Assert.Null(payment.ReferenceNumber);
    }

    [Fact]
    public void Constructor_SupportsAllPaymentMethods()
    {
        var methods = Enum.GetValues<PaymentMethod>();

        foreach (var method in methods)
        {
            var payment = new Payment(
                Guid.NewGuid(),
                method,
                new Money(100),
                DateTime.UtcNow,
                Guid.NewGuid().ToString());

            Assert.Equal(method, payment.Method);
        }
    }
}
