using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.Entities;

public class SalePaymentTests
{
    [Fact]
    public void AddPayment_ToConfirmedSale_AddsPayment()
    {
        var sale = CreateConfirmedSale();
        var payment = CreatePayment(sale.Id);

        sale.AddPayment(payment);

        var addedPayment = Assert.Single(sale.Payments);
        Assert.Same(payment, addedPayment);
    }

    [Fact]
    public void AddPayment_ToDraftSale_ThrowsSaleDomainException()
    {
        var sale = CreateDraftSale();
        var payment = CreatePayment(sale.Id);

        var exception = Assert.Throws<SaleDomainException>(
            () => sale.AddPayment(payment));

        Assert.Equal(
            "Payments can only be added to a confirmed sale.",
            exception.Message);
    }

    [Fact]
    public void AddPayment_ToCompletedSale_ThrowsSaleDomainException()
    {
        var sale = CreateConfirmedSale();
        sale.Complete();

        var payment = CreatePayment(sale.Id);

        var exception = Assert.Throws<SaleDomainException>(
            () => sale.AddPayment(payment));

        Assert.Equal(
            "Payments can only be added to a confirmed sale.",
            exception.Message);
    }

    [Fact]
    public void AddPayment_ToCancelledSale_ThrowsSaleDomainException()
    {
        var sale = CreateDraftSale();
        sale.Cancel();

        var payment = CreatePayment(sale.Id);

        var exception = Assert.Throws<SaleDomainException>(
            () => sale.AddPayment(payment));

        Assert.Equal(
            "Payments can only be added to a confirmed sale.",
            exception.Message);
    }

    [Fact]
    public void AddPayment_ToVoidedSale_ThrowsSaleDomainException()
    {
        var sale = CreateConfirmedSale();
        sale.Complete();
        sale.Void();

        var payment = CreatePayment(sale.Id);

        var exception = Assert.Throws<SaleDomainException>(
            () => sale.AddPayment(payment));

        Assert.Equal(
            "Payments can only be added to a confirmed sale.",
            exception.Message);
    }

    [Fact]
    public void AddPayment_WithPaymentBelongingToAnotherSale_ThrowsSaleDomainException()
    {
        var sale = CreateConfirmedSale();
        var payment = CreatePayment(Guid.NewGuid());

        var exception = Assert.Throws<SaleDomainException>(
            () => sale.AddPayment(payment));

        Assert.Equal(
            "Payment does not belong to this sale.",
            exception.Message);
    }

    [Fact]
    public void AddPayment_WithNullPayment_ThrowsSaleDomainException()
    {
        var sale = CreateConfirmedSale();

        var exception = Assert.Throws<SaleDomainException>(
            () => sale.AddPayment(null!));

        Assert.Equal(
            "Payment cannot be null.",
            exception.Message);
    }

    private static Sale CreateDraftSale()
    {
        return new Sale(
            null,
            DateTime.UtcNow);
    }

    private static Sale CreateConfirmedSale()
    {
        var sale = CreateDraftSale();

        var line = new SaleLine(
            sale.Id,
            Guid.NewGuid(),
            1,
            new Money(100));

        sale.AddLine(line);
        sale.Confirm();

        return sale;
    }

    private static Payment CreatePayment(Guid saleId)
    {
        return new Payment(
            saleId,
            PaymentMethod.Cash,
            new Money(100),
            DateTime.UtcNow,
            Guid.NewGuid().ToString());
    }
}
