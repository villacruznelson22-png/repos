using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.PurchaseOrders;

public class PurchaseOrderTests
{
    private static PurchaseOrder CreatePurchaseOrder()
    {
        return new PurchaseOrder(
            Guid.NewGuid(),
            DateTime.UtcNow);
    }

    private static PurchaseOrderLine CreateLine(
        Guid purchaseOrderId,
        Guid? productId = null)
    {
        return new PurchaseOrderLine(
            purchaseOrderId,
            productId ?? Guid.NewGuid(),
            10,
            new Money(180));
    }

    [Fact]
    public void Constructor_ShouldCreateDraftPurchaseOrder()
    {
        var supplierId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var purchaseOrder = new PurchaseOrder(
            supplierId,
            occurredAt,
            "PO-001",
            "Test notes");

        Assert.NotEqual(Guid.Empty, purchaseOrder.Id);
        Assert.Equal(supplierId, purchaseOrder.SupplierId);
        Assert.Equal(occurredAt, purchaseOrder.OccurredAt);
        Assert.Equal("PO-001", purchaseOrder.ReferenceNumber);
        Assert.Equal("Test notes", purchaseOrder.Notes);
        Assert.Equal(
            PurchaseOrderStatus.Draft,
            purchaseOrder.Status);
        Assert.Empty(purchaseOrder.Lines);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSupplierIdIsEmpty()
    {
        Assert.Throws<PurchaseOrderDomainException>(
            () => new PurchaseOrder(
                Guid.Empty,
                DateTime.UtcNow));
    }

    [Fact]
    public void AddLine_ShouldAddLine()
    {
        var purchaseOrder = CreatePurchaseOrder();
        var line = CreateLine(purchaseOrder.Id);

        purchaseOrder.AddLine(line);

        Assert.Single(purchaseOrder.Lines);
        Assert.Contains(line, purchaseOrder.Lines);
    }

    [Fact]
    public void AddLine_ShouldThrow_WhenSameProductAlreadyExists()
    {
        var purchaseOrder = CreatePurchaseOrder();
        var productId = Guid.NewGuid();

        var firstLine = CreateLine(
            purchaseOrder.Id,
            productId);

        var secondLine = CreateLine(
            purchaseOrder.Id,
            productId);

        purchaseOrder.AddLine(firstLine);

        Assert.Throws<PurchaseOrderDomainException>(
            () => purchaseOrder.AddLine(secondLine));
    }

    [Fact]
    public void AddLine_ShouldThrow_WhenLineBelongsToAnotherPurchaseOrder()
    {
        var purchaseOrder = CreatePurchaseOrder();

        var line = CreateLine(
            Guid.NewGuid());

        Assert.Throws<PurchaseOrderDomainException>(
            () => purchaseOrder.AddLine(line));
    }

    [Fact]
    public void RemoveLine_ShouldRemoveLine()
    {
        var purchaseOrder = CreatePurchaseOrder();
        var line = CreateLine(purchaseOrder.Id);

        purchaseOrder.AddLine(line);

        purchaseOrder.RemoveLine(line.Id);

        Assert.Empty(purchaseOrder.Lines);
    }

    [Fact]
    public void ChangeLineQuantity_ShouldChangeQuantity()
    {
        var purchaseOrder = CreatePurchaseOrder();
        var line = CreateLine(purchaseOrder.Id);

        purchaseOrder.AddLine(line);

        purchaseOrder.ChangeLineQuantity(
            line.Id,
            25);

        Assert.Equal(25, line.Quantity);
    }

    [Fact]
    public void ChangeLineUnitCost_ShouldChangeCost()
    {
        var purchaseOrder = CreatePurchaseOrder();
        var line = CreateLine(purchaseOrder.Id);

        purchaseOrder.AddLine(line);

        purchaseOrder.ChangeLineUnitCost(
            line.Id,
            new Money(185));

        Assert.Equal(185, line.UnitCost.Value);
    }

    [Fact]
    public void Post_ShouldChangeStatusToPosted()
    {
        var purchaseOrder = CreatePurchaseOrder();
        purchaseOrder.AddLine(
            CreateLine(purchaseOrder.Id));

        purchaseOrder.Post();

        Assert.Equal(
            PurchaseOrderStatus.Posted,
            purchaseOrder.Status);
    }

    [Fact]
    public void Post_ShouldThrow_WhenThereAreNoLines()
    {
        var purchaseOrder = CreatePurchaseOrder();

        Assert.Throws<PurchaseOrderDomainException>(
            () => purchaseOrder.Post());
    }

    [Fact]
    public void Post_ShouldThrow_WhenAlreadyPosted()
    {
        var purchaseOrder = CreatePurchaseOrder();

        purchaseOrder.AddLine(
            CreateLine(purchaseOrder.Id));

        purchaseOrder.Post();

        Assert.Throws<PurchaseOrderDomainException>(
            () => purchaseOrder.Post());
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        var purchaseOrder = CreatePurchaseOrder();

        purchaseOrder.Cancel();

        Assert.Equal(
            PurchaseOrderStatus.Cancelled,
            purchaseOrder.Status);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenAlreadyCancelled()
    {
        var purchaseOrder = CreatePurchaseOrder();

        purchaseOrder.Cancel();

        Assert.Throws<PurchaseOrderDomainException>(
            () => purchaseOrder.Cancel());
    }

    [Fact]
    public void PostedPurchaseOrder_ShouldNotBeEditable()
    {
        var purchaseOrder = CreatePurchaseOrder();

        purchaseOrder.AddLine(
            CreateLine(purchaseOrder.Id));

        purchaseOrder.Post();

        Assert.Throws<PurchaseOrderDomainException>(
            () => purchaseOrder.AddLine(
                CreateLine(purchaseOrder.Id)));
    }

    [Fact]
    public void CancelledPurchaseOrder_ShouldNotBeEditable()
    {
        var purchaseOrder = CreatePurchaseOrder();

        purchaseOrder.Cancel();

        Assert.Throws<PurchaseOrderDomainException>(
            () => purchaseOrder.AddLine(
                CreateLine(purchaseOrder.Id)));
    }
}