using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.Sales;

public class SaleTests
{
    private static Sale CreateSale()
    {
        return new Sale(
            null,
            DateTime.UtcNow);
    }

    private static SaleLine CreateLine(
        Guid saleId,
        Guid? productId = null)
    {
        return new SaleLine(
            saleId,
            productId ?? Guid.NewGuid(),
            10,
            new Money(180));
    }

    [Fact]
    public void Constructor_ShouldCreateDraftSale()
    {
        var customerId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var sale = new Sale(
            customerId,
            occurredAt,
            "SALE-001",
            "Test notes");

        Assert.NotEqual(Guid.Empty, sale.Id);
        Assert.Equal(customerId, sale.CustomerId);
        Assert.Equal(occurredAt, sale.OccurredAt);
        Assert.Equal("SALE-001", sale.ReferenceNumber);
        Assert.Equal("Test notes", sale.Notes);
        Assert.Equal(
            SaleStatus.Draft,
            sale.Status);
        Assert.Empty(sale.Lines);
    }

    [Fact]
    public void Constructor_ShouldAllowWalkInSale()
    {
        var sale = new Sale(
            null,
            DateTime.UtcNow);

        Assert.Null(sale.CustomerId);
        Assert.Equal(
            SaleStatus.Draft,
            sale.Status);
    }

    [Fact]
    public void AddLine_ShouldAddLine()
    {
        var sale = CreateSale();
        var line = CreateLine(sale.Id);

        sale.AddLine(line);

        Assert.Single(sale.Lines);
        Assert.Contains(line, sale.Lines);
    }

    [Fact]
    public void AddLine_ShouldAllowDuplicateProducts()
    {
        var sale = CreateSale();
        var productId = Guid.NewGuid();

        var firstLine = CreateLine(
            sale.Id,
            productId);

        var secondLine = CreateLine(
            sale.Id,
            productId);

        sale.AddLine(firstLine);
        sale.AddLine(secondLine);

        Assert.Equal(2, sale.Lines.Count);
        Assert.All(
            sale.Lines,
            line => Assert.Equal(
                productId,
                line.ProductId));
    }

    [Fact]
    public void AddLine_ShouldThrow_WhenLineBelongsToAnotherSale()
    {
        var sale = CreateSale();

        var line = CreateLine(
            Guid.NewGuid());

        Assert.Throws<SaleDomainException>(
            () => sale.AddLine(line));
    }

    [Fact]
    public void RemoveLine_ShouldRemoveLine()
    {
        var sale = CreateSale();
        var line = CreateLine(sale.Id);

        sale.AddLine(line);

        sale.RemoveLine(line.Id);

        Assert.Empty(sale.Lines);
    }

    [Fact]
    public void ChangeLineQuantity_ShouldChangeQuantity()
    {
        var sale = CreateSale();
        var line = CreateLine(sale.Id);

        sale.AddLine(line);

        sale.ChangeLineQuantity(
            line.Id,
            25);

        Assert.Equal(25, line.Quantity);
    }

    [Fact]
    public void ChangeLineUnitSellingPrice_ShouldChangePrice()
    {
        var sale = CreateSale();
        var line = CreateLine(sale.Id);

        sale.AddLine(line);

        sale.ChangeLineUnitSellingPrice(
            line.Id,
            new Money(185));

        Assert.Equal(
            185,
            line.UnitSellingPrice.Value);
    }

    [Fact]
    public void Confirm_ShouldChangeStatusToConfirmed()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();

        Assert.Equal(
            SaleStatus.Confirmed,
            sale.Status);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenThereAreNoLines()
    {
        var sale = CreateSale();

        Assert.Throws<SaleDomainException>(
            () => sale.Confirm());
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenAlreadyConfirmed()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();

        Assert.Throws<SaleDomainException>(
            () => sale.Confirm());
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        var sale = CreateSale();

        sale.Cancel();

        Assert.Equal(
            SaleStatus.Cancelled,
            sale.Status);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenAlreadyCancelled()
    {
        var sale = CreateSale();

        sale.Cancel();

        Assert.Throws<SaleDomainException>(
            () => sale.Cancel());
    }

    [Fact]
    public void ConfirmedSale_ShouldNotBeEditable()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();

        Assert.Throws<SaleDomainException>(
            () => sale.AddLine(
                CreateLine(sale.Id)));
    }

    [Fact]
    public void CancelledSale_ShouldNotBeEditable()
    {
        var sale = CreateSale();

        sale.Cancel();

        Assert.Throws<SaleDomainException>(
            () => sale.AddLine(
                CreateLine(sale.Id)));
    }

    [Fact]
    public void ChangeCustomer_ShouldChangeCustomer()
    {
        var sale = CreateSale();

        var customerId = Guid.NewGuid();

        sale.ChangeCustomer(customerId);

        Assert.Equal(
            customerId,
            sale.CustomerId);
    }
}