using System.Reflection;
using Moq;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.PurchaseOrders.Queries.GetPurchaseOrderById;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Tests.PurchaseOrders.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPurchaseOrderDto()
    {
        var supplier = new Supplier(
            "Test Supplier");

        var purchaseOrder = new PurchaseOrder(
            supplier.Id,
            DateTime.UtcNow,
            "PO-001",
            "Test notes");

        SetNavigationProperty(
            purchaseOrder,
            nameof(PurchaseOrder.Supplier),
            supplier);

        var product = new Product(
            "Coke Mismo",
            null,
            new Money(195),
            new Money(190));

        var line = new PurchaseOrderLine(
            purchaseOrder.Id,
            product.Id,
            20,
            new Money(180));

        SetNavigationProperty(
            line,
            nameof(PurchaseOrderLine.Product),
            product);

        purchaseOrder.AddLine(line);

        var repository =
            new Mock<IPurchaseOrderRepository>();

        repository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrder.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        var handler =
            new GetPurchaseOrderByIdHandler(
                repository.Object);

        var result =
            await handler.Handle(
                new GetPurchaseOrderByIdQuery(
                    purchaseOrder.Id),
                CancellationToken.None);

        Assert.Equal(
            purchaseOrder.Id,
            result.Id);

        Assert.Equal(
            supplier.Id,
            result.SupplierId);

        Assert.Equal(
            "Test Supplier",
            result.SupplierName);

        Assert.Equal(
            purchaseOrder.OccurredAt,
            result.OccurredAt);

        Assert.Equal(
            "PO-001",
            result.ReferenceNumber);

        Assert.Equal(
            "Test notes",
            result.Notes);

        Assert.Equal(
            "Draft",
            result.Status);

        Assert.Equal(
            purchaseOrder.CreatedAt,
            result.CreatedAt);

        Assert.Single(
            result.Lines);

        var resultLine =
            result.Lines.Single();

        Assert.Equal(
            line.Id,
            resultLine.Id);

        Assert.Equal(
            product.Id,
            resultLine.ProductId);

        Assert.Equal(
            "Coke Mismo",
            resultLine.ProductName);

        Assert.Equal(
            20,
            resultLine.Quantity);

        Assert.Equal(
            180,
            resultLine.UnitCost);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenPurchaseOrderDoesNotExist()
    {
        var purchaseOrderId =
            Guid.NewGuid();

        var repository =
            new Mock<IPurchaseOrderRepository>();

        repository
            .Setup(x => x.GetByIdWithLinesAsync(
                purchaseOrderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder?)null);

        var handler =
            new GetPurchaseOrderByIdHandler(
                repository.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(
                new GetPurchaseOrderByIdQuery(
                    purchaseOrderId),
                CancellationToken.None));

        repository.Verify(
            x => x.GetByIdWithLinesAsync(
                purchaseOrderId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static void SetNavigationProperty(
        object entity,
        string propertyName,
        object value)
    {
        var property =
            entity.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        if (property is null)
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}' was not found on '{entity.GetType().Name}'.");
        }

        property.SetValue(
            entity,
            value);
    }
}