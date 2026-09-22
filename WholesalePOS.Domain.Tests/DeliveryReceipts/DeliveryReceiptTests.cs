using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.DeliveryReceipts;

public class DeliveryReceiptTests
{
    [Fact]
    public void Constructor_ShouldCreateDraftDeliveryReceipt()
    {
        var purchaseOrderId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var deliveryReceipt = new DeliveryReceipt(
            purchaseOrderId,
            occurredAt,
            "DR-001",
            "Test delivery");

        Assert.NotEqual(
            Guid.Empty,
            deliveryReceipt.Id);

        Assert.Equal(
            purchaseOrderId,
            deliveryReceipt.PurchaseOrderId);

        Assert.Equal(
            occurredAt,
            deliveryReceipt.OccurredAt);

        Assert.Equal(
            "DR-001",
            deliveryReceipt.ReferenceNumber);

        Assert.Equal(
            "Test delivery",
            deliveryReceipt.Notes);

        Assert.Equal(
            DeliveryReceiptStatus.Draft,
            deliveryReceipt.Status);

        Assert.Empty(
            deliveryReceipt.Lines);

        Assert.NotEqual(
            default,
            deliveryReceipt.CreatedAt);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPurchaseOrderIdIsEmpty()
    {
        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => new DeliveryReceipt(
                    Guid.Empty,
                    DateTime.UtcNow));

        Assert.Equal(
            "Purchase order ID cannot be empty.",
            exception.Message);
    }

    [Fact]
    public void ChangeOccurredAt_ShouldUpdateOccurredAt()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var newOccurredAt = new DateTime(
            2026,
            9,
            18,
            15,
            30,
            0,
            DateTimeKind.Utc);

        deliveryReceipt.ChangeOccurredAt(
            newOccurredAt);

        Assert.Equal(
            newOccurredAt,
            deliveryReceipt.OccurredAt);
    }

    [Fact]
    public void ChangeReferenceNumber_ShouldUpdateReferenceNumber()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        deliveryReceipt.ChangeReferenceNumber(
            "DR-UPDATED");

        Assert.Equal(
            "DR-UPDATED",
            deliveryReceipt.ReferenceNumber);
    }

    [Fact]
    public void ChangeReferenceNumber_ShouldAllowNull()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        deliveryReceipt.ChangeReferenceNumber(
            null);

        Assert.Null(
            deliveryReceipt.ReferenceNumber);
    }

    [Fact]
    public void ChangeNotes_ShouldUpdateNotes()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        deliveryReceipt.ChangeNotes(
            "Updated notes");

        Assert.Equal(
            "Updated notes",
            deliveryReceipt.Notes);
    }

    [Fact]
    public void ChangeNotes_ShouldAllowNull()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        deliveryReceipt.ChangeNotes(
            null);

        Assert.Null(
            deliveryReceipt.Notes);
    }

    [Fact]
    public void AddLine_ShouldAddLine()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var line =
            CreateLine(deliveryReceipt.Id);

        deliveryReceipt.AddLine(line);

        Assert.Single(
            deliveryReceipt.Lines);

        Assert.Contains(
            line,
            deliveryReceipt.Lines);
    }

    [Fact]
    public void AddLine_ShouldAllowSameProductMoreThanOnce()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var productId = Guid.NewGuid();

        var firstLine = new DeliveryReceiptLine(
            deliveryReceipt.Id,
            productId,
            100,
            new Money(180),
            new DateTime(2026, 12, 15));

        var secondLine = new DeliveryReceiptLine(
            deliveryReceipt.Id,
            productId,
            50,
            new Money(180),
            new DateTime(2027, 1, 20));

        deliveryReceipt.AddLine(firstLine);
        deliveryReceipt.AddLine(secondLine);

        Assert.Equal(
            2,
            deliveryReceipt.Lines.Count);
    }

    [Fact]
    public void AddLine_ShouldThrow_WhenLineIsNull()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.AddLine(null!));

        Assert.Equal(
            "Delivery receipt line cannot be null.",
            exception.Message);
    }

    [Fact]
    public void AddLine_ShouldThrow_WhenLineBelongsToAnotherDeliveryReceipt()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var line =
            CreateLine(Guid.NewGuid());

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.AddLine(line));

        Assert.Equal(
            "Delivery receipt line does not belong to this delivery receipt.",
            exception.Message);
    }

    [Fact]
    public void RemoveLine_ShouldRemoveLine()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var line =
            CreateLine(deliveryReceipt.Id);

        deliveryReceipt.AddLine(line);

        deliveryReceipt.RemoveLine(
            line.Id);

        Assert.Empty(
            deliveryReceipt.Lines);
    }

    [Fact]
    public void RemoveLine_ShouldThrow_WhenLineDoesNotExist()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.RemoveLine(
                    Guid.NewGuid()));

        Assert.Equal(
            "Delivery receipt line was not found.",
            exception.Message);
    }

    [Fact]
    public void ChangeLineQuantity_ShouldUpdateLine()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var line =
            CreateLine(deliveryReceipt.Id);

        deliveryReceipt.AddLine(line);

        deliveryReceipt.ChangeLineQuantity(
            line.Id,
            25);

        Assert.Equal(
            25,
            line.Quantity);
    }

    [Fact]
    public void ChangeLineUnitCost_ShouldUpdateLine()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var line =
            CreateLine(deliveryReceipt.Id);

        deliveryReceipt.AddLine(line);

        deliveryReceipt.ChangeLineUnitCost(
            line.Id,
            new Money(195));

        Assert.Equal(
            195,
            line.UnitCost.Value);
    }

    [Fact]
    public void ChangeLineExpirationDate_ShouldUpdateLine()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var line =
            CreateLine(deliveryReceipt.Id);

        deliveryReceipt.AddLine(line);

        var expirationDate =
            new DateTime(2027, 2, 1);

        deliveryReceipt.ChangeLineExpirationDate(
            line.Id,
            expirationDate);

        Assert.Equal(
            expirationDate,
            line.ExpirationDate);
    }

    [Fact]
    public void ChangeLineQuantity_ShouldThrow_WhenLineDoesNotExist()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.ChangeLineQuantity(
                    Guid.NewGuid(),
                    20));

        Assert.Equal(
            "Delivery receipt line was not found.",
            exception.Message);
    }

    [Fact]
    public void ChangeLineUnitCost_ShouldThrow_WhenLineDoesNotExist()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.ChangeLineUnitCost(
                    Guid.NewGuid(),
                    new Money(190)));

        Assert.Equal(
            "Delivery receipt line was not found.",
            exception.Message);
    }

    [Fact]
    public void ChangeLineExpirationDate_ShouldThrow_WhenLineDoesNotExist()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.ChangeLineExpirationDate(
                    Guid.NewGuid(),
                    DateTime.UtcNow));

        Assert.Equal(
            "Delivery receipt line was not found.",
            exception.Message);
    }

    [Fact]
    public void Post_ShouldChangeStatusToPosted()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        deliveryReceipt.AddLine(
            CreateLine(deliveryReceipt.Id));

        deliveryReceipt.Post();

        Assert.Equal(
            DeliveryReceiptStatus.Posted,
            deliveryReceipt.Status);
    }

    [Fact]
    public void Post_ShouldThrow_WhenThereAreNoLines()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.Post());

        Assert.Equal(
            "Delivery receipt must contain at least one line.",
            exception.Message);

        Assert.Equal(
            DeliveryReceiptStatus.Draft,
            deliveryReceipt.Status);
    }

    [Fact]
    public void Post_ShouldThrow_WhenAlreadyPosted()
    {
        var deliveryReceipt =
            CreatePostedDeliveryReceipt();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.Post());

        Assert.Equal(
            "Only a draft delivery receipt can be posted.",
            exception.Message);
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        deliveryReceipt.Cancel();

        Assert.Equal(
            DeliveryReceiptStatus.Cancelled,
            deliveryReceipt.Status);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenPosted()
    {
        var deliveryReceipt =
            CreatePostedDeliveryReceipt();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.Cancel());

        Assert.Equal(
            "Only a draft delivery receipt can be cancelled.",
            exception.Message);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenAlreadyCancelled()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        deliveryReceipt.Cancel();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.Cancel());

        Assert.Equal(
            "Only a draft delivery receipt can be cancelled.",
            exception.Message);
    }

    [Fact]
    public void PostedDeliveryReceipt_ShouldNotAllowHeaderChanges()
    {
        var deliveryReceipt =
            CreatePostedDeliveryReceipt();

        Assert.Throws<DeliveryReceiptDomainException>(
            () => deliveryReceipt.ChangeOccurredAt(
                DateTime.UtcNow));

        Assert.Throws<DeliveryReceiptDomainException>(
            () => deliveryReceipt.ChangeReferenceNumber(
                "DR-UPDATED"));

        Assert.Throws<DeliveryReceiptDomainException>(
            () => deliveryReceipt.ChangeNotes(
                "Updated"));
    }

    [Fact]
    public void PostedDeliveryReceipt_ShouldNotAllowAddingLine()
    {
        var deliveryReceipt =
            CreatePostedDeliveryReceipt();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.AddLine(
                    CreateLine(deliveryReceipt.Id)));

        Assert.Equal(
            "Only a draft delivery receipt can be modified.",
            exception.Message);
    }

    [Fact]
    public void PostedDeliveryReceipt_ShouldNotAllowRemovingLine()
    {
        var deliveryReceipt =
            CreatePostedDeliveryReceipt();

        var line =
            deliveryReceipt.Lines.Single();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.RemoveLine(
                    line.Id));

        Assert.Equal(
            "Only a draft delivery receipt can be modified.",
            exception.Message);
    }

    [Fact]
    public void PostedDeliveryReceipt_ShouldNotAllowChangingLineQuantity()
    {
        var deliveryReceipt =
            CreatePostedDeliveryReceipt();

        var line =
            deliveryReceipt.Lines.Single();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.ChangeLineQuantity(
                    line.Id,
                    50));

        Assert.Equal(
            "Only a draft delivery receipt can be modified.",
            exception.Message);
    }

    [Fact]
    public void PostedDeliveryReceipt_ShouldNotAllowChangingLineUnitCost()
    {
        var deliveryReceipt =
            CreatePostedDeliveryReceipt();

        var line =
            deliveryReceipt.Lines.Single();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.ChangeLineUnitCost(
                    line.Id,
                    new Money(200)));

        Assert.Equal(
            "Only a draft delivery receipt can be modified.",
            exception.Message);
    }

    [Fact]
    public void PostedDeliveryReceipt_ShouldNotAllowChangingLineExpirationDate()
    {
        var deliveryReceipt =
            CreatePostedDeliveryReceipt();

        var line =
            deliveryReceipt.Lines.Single();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.ChangeLineExpirationDate(
                    line.Id,
                    DateTime.UtcNow));

        Assert.Equal(
            "Only a draft delivery receipt can be modified.",
            exception.Message);
    }

    [Fact]
    public void CancelledDeliveryReceipt_ShouldNotAllowHeaderChanges()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        deliveryReceipt.Cancel();

        Assert.Throws<DeliveryReceiptDomainException>(
            () => deliveryReceipt.ChangeOccurredAt(
                DateTime.UtcNow));

        Assert.Throws<DeliveryReceiptDomainException>(
            () => deliveryReceipt.ChangeReferenceNumber(
                "DR-UPDATED"));

        Assert.Throws<DeliveryReceiptDomainException>(
            () => deliveryReceipt.ChangeNotes(
                "Updated"));
    }

    [Fact]
    public void CancelledDeliveryReceipt_ShouldNotAllowAddingLine()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        deliveryReceipt.Cancel();

        var exception =
            Assert.Throws<DeliveryReceiptDomainException>(
                () => deliveryReceipt.AddLine(
                    CreateLine(deliveryReceipt.Id)));

        Assert.Equal(
            "Only a draft delivery receipt can be modified.",
            exception.Message);
    }

    private static DeliveryReceipt CreateDeliveryReceipt()
    {
        return new DeliveryReceipt(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "DR-001",
            "Test");
    }

    private static DeliveryReceipt CreatePostedDeliveryReceipt()
    {
        var deliveryReceipt =
            CreateDeliveryReceipt();

        deliveryReceipt.AddLine(
            CreateLine(deliveryReceipt.Id));

        deliveryReceipt.Post();

        return deliveryReceipt;
    }

    private static DeliveryReceiptLine CreateLine(
        Guid deliveryReceiptId)
    {
        return new DeliveryReceiptLine(
            deliveryReceiptId,
            Guid.NewGuid(),
            10,
            new Money(180));
    }
}