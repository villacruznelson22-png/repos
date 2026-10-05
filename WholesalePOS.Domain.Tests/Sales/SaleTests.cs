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

        Assert.NotEqual(
            Guid.Empty,
            sale.Id);

        Assert.Equal(
            customerId,
            sale.CustomerId);

        Assert.Equal(
            occurredAt,
            sale.OccurredAt);

        Assert.Equal(
            "SALE-001",
            sale.ReferenceNumber);

        Assert.Equal(
            "Test notes",
            sale.Notes);

        Assert.Equal(
            SaleStatus.Draft,
            sale.Status);

        Assert.NotEqual(
            default,
            sale.CreatedAt);

        Assert.Null(sale.ConfirmedAt);
        Assert.Null(sale.CompletedAt);
        Assert.Null(sale.CancelledAt);
        Assert.Null(sale.VoidedAt);

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
    public void AddLine_ShouldThrow_WhenDuplicateProductIsAdded()
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

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.AddLine(secondLine));

        Assert.Equal(
            "The same product cannot be added more than once to a sale.",
            exception.Message);

        Assert.Single(sale.Lines);
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

        Assert.Equal(
            25,
            line.Quantity);
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

        Assert.NotNull(sale.ConfirmedAt);

        Assert.Null(sale.CompletedAt);
        Assert.Null(sale.CancelledAt);
        Assert.Null(sale.VoidedAt);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenThereAreNoLines()
    {
        var sale = CreateSale();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.Confirm());

        Assert.Equal(
            "Sale must contain at least one line.",
            exception.Message);

        Assert.Equal(
            SaleStatus.Draft,
            sale.Status);

        Assert.Null(sale.ConfirmedAt);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenAlreadyConfirmed()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.Confirm());

        Assert.Equal(
            "Only a draft sale can be confirmed.",
            exception.Message);

        Assert.Equal(
            SaleStatus.Confirmed,
            sale.Status);
    }

    [Fact]
    public void Complete_ShouldChangeStatusToCompleted()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();

        sale.Complete();

        Assert.Equal(
            SaleStatus.Completed,
            sale.Status);

        Assert.NotNull(sale.ConfirmedAt);
        Assert.NotNull(sale.CompletedAt);

        Assert.Null(sale.CancelledAt);
        Assert.Null(sale.VoidedAt);
    }

    [Fact]
    public void Complete_ShouldThrow_WhenSaleIsDraft()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.Complete());

        Assert.Equal(
            "Only a confirmed sale can be completed.",
            exception.Message);

        Assert.Equal(
            SaleStatus.Draft,
            sale.Status);

        Assert.Null(sale.CompletedAt);
    }

    [Fact]
    public void Complete_ShouldThrow_WhenAlreadyCompleted()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();
        sale.Complete();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.Complete());

        Assert.Equal(
            "Only a confirmed sale can be completed.",
            exception.Message);

        Assert.Equal(
            SaleStatus.Completed,
            sale.Status);
    }

    [Fact]
    public void Cancel_ShouldChangeDraftSaleToCancelled()
    {
        var sale = CreateSale();

        sale.Cancel();

        Assert.Equal(
            SaleStatus.Cancelled,
            sale.Status);

        Assert.NotNull(sale.CancelledAt);

        Assert.Null(sale.ConfirmedAt);
        Assert.Null(sale.CompletedAt);
        Assert.Null(sale.VoidedAt);
    }

    [Fact]
    public void Cancel_ShouldChangeConfirmedSaleToCancelled()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();

        sale.Cancel();

        Assert.Equal(
            SaleStatus.Cancelled,
            sale.Status);

        Assert.NotNull(sale.ConfirmedAt);
        Assert.NotNull(sale.CancelledAt);

        Assert.Null(sale.CompletedAt);
        Assert.Null(sale.VoidedAt);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenAlreadyCancelled()
    {
        var sale = CreateSale();

        sale.Cancel();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.Cancel());

        Assert.Equal(
            "Only a draft or confirmed sale can be cancelled.",
            exception.Message);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenSaleIsCompleted()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();
        sale.Complete();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.Cancel());

        Assert.Equal(
            "Only a draft or confirmed sale can be cancelled.",
            exception.Message);

        Assert.Equal(
            SaleStatus.Completed,
            sale.Status);
    }

    [Fact]
    public void Void_ShouldChangeCompletedSaleToVoided()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();
        sale.Complete();

        sale.Void();

        Assert.Equal(
            SaleStatus.Voided,
            sale.Status);

        Assert.NotNull(sale.ConfirmedAt);
        Assert.NotNull(sale.CompletedAt);
        Assert.NotNull(sale.VoidedAt);

        Assert.Null(sale.CancelledAt);
    }

    [Fact]
    public void Void_ShouldThrow_WhenSaleIsDraft()
    {
        var sale = CreateSale();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.Void());

        Assert.Equal(
            "Only a completed sale can be voided.",
            exception.Message);
    }

    [Fact]
    public void Void_ShouldThrow_WhenSaleIsConfirmed()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.Void());

        Assert.Equal(
            "Only a completed sale can be voided.",
            exception.Message);

        Assert.Equal(
            SaleStatus.Confirmed,
            sale.Status);
    }

    [Fact]
    public void Void_ShouldThrow_WhenSaleIsCancelled()
    {
        var sale = CreateSale();

        sale.Cancel();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.Void());

        Assert.Equal(
            "Only a completed sale can be voided.",
            exception.Message);

        Assert.Equal(
            SaleStatus.Cancelled,
            sale.Status);
    }

    [Fact]
    public void Void_ShouldThrow_WhenAlreadyVoided()
    {
        var sale = CreateSale();

        sale.AddLine(
            CreateLine(sale.Id));

        sale.Confirm();
        sale.Complete();
        sale.Void();

        var exception =
            Assert.Throws<SaleDomainException>(
                () => sale.Void());

        Assert.Equal(
            "Only a completed sale can be voided.",
            exception.Message);

        Assert.Equal(
            SaleStatus.Voided,
            sale.Status);
    }

    [Fact]
    public void DraftSale_ShouldBeEditable()
    {
        var sale = CreateSale();

        var line = CreateLine(sale.Id);

        sale.AddLine(line);

        sale.ChangeCustomer(
            Guid.NewGuid());

        sale.ChangeOccurredAt(
            DateTime.UtcNow.AddMinutes(5));

        sale.ChangeReferenceNumber(
            "SALE-002");

        sale.ChangeNotes(
            "Updated notes");

        sale.ChangeLineQuantity(
            line.Id,
            20);

        sale.ChangeLineUnitSellingPrice(
            line.Id,
            new Money(190));

        Assert.Single(sale.Lines);

        Assert.Equal(
            20,
            line.Quantity);

        Assert.Equal(
            190,
            line.UnitSellingPrice.Value);
    }

    [Fact]
    public void ConfirmedSale_ShouldStillBeEditable()
    {
        var sale = CreateSale();

        var firstLine = CreateLine(sale.Id);

        sale.AddLine(firstLine);

        sale.Confirm();

        var secondLine = CreateLine(sale.Id);

        sale.AddLine(secondLine);

        sale.ChangeCustomer(
            Guid.NewGuid());

        sale.ChangeOccurredAt(
            DateTime.UtcNow.AddMinutes(5));

        sale.ChangeReferenceNumber(
            "SALE-003");

        sale.ChangeNotes(
            "Confirmed but updated");

        sale.ChangeLineQuantity(
            firstLine.Id,
            15);

        sale.ChangeLineUnitSellingPrice(
            firstLine.Id,
            new Money(195));

        Assert.Equal(
            2,
            sale.Lines.Count);

        Assert.Equal(
            15,
            firstLine.Quantity);

        Assert.Equal(
            195,
            firstLine.UnitSellingPrice.Value);
    }

    [Fact]
    public void CompletedSale_ShouldNotBeEditable()
    {
        var sale = CreateSale();

        var line = CreateLine(sale.Id);

        sale.AddLine(line);

        sale.Confirm();
        sale.Complete();

        Assert.Throws<SaleDomainException>(
            () => sale.AddLine(
                CreateLine(sale.Id)));

        Assert.Throws<SaleDomainException>(
            () => sale.RemoveLine(line.Id));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeLineQuantity(
                line.Id,
                20));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeLineUnitSellingPrice(
                line.Id,
                new Money(200)));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeCustomer(
                Guid.NewGuid()));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeOccurredAt(
                DateTime.UtcNow));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeReferenceNumber(
                "UPDATED"));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeNotes(
                "UPDATED"));
    }

    [Fact]
    public void CancelledSale_ShouldNotBeEditable()
    {
        var sale = CreateSale();

        var line = CreateLine(sale.Id);

        sale.AddLine(line);

        sale.Cancel();

        Assert.Throws<SaleDomainException>(
            () => sale.AddLine(
                CreateLine(sale.Id)));

        Assert.Throws<SaleDomainException>(
            () => sale.RemoveLine(line.Id));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeLineQuantity(
                line.Id,
                20));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeLineUnitSellingPrice(
                line.Id,
                new Money(200)));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeCustomer(
                Guid.NewGuid()));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeOccurredAt(
                DateTime.UtcNow));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeReferenceNumber(
                "UPDATED"));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeNotes(
                "UPDATED"));
    }

    [Fact]
    public void VoidedSale_ShouldNotBeEditable()
    {
        var sale = CreateSale();

        var line = CreateLine(sale.Id);

        sale.AddLine(line);

        sale.Confirm();
        sale.Complete();
        sale.Void();

        Assert.Throws<SaleDomainException>(
            () => sale.AddLine(
                CreateLine(sale.Id)));

        Assert.Throws<SaleDomainException>(
            () => sale.RemoveLine(line.Id));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeLineQuantity(
                line.Id,
                20));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeLineUnitSellingPrice(
                line.Id,
                new Money(200)));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeCustomer(
                Guid.NewGuid()));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeOccurredAt(
                DateTime.UtcNow));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeReferenceNumber(
                "UPDATED"));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeNotes(
                "UPDATED"));
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