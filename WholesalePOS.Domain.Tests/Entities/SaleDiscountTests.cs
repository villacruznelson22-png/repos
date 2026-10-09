using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.Entities;

public class SaleDiscountTests
{
    [Fact]
    public void AddLineAndSaleDiscounts_CalculatesNetTotalInOrder()
    {
        var (sale, line, userId) = CreateSale(2, 100m);

        sale.AddDiscount(new SaleDiscount(
            sale.Id, line.Id, DiscountScope.SaleLine,
            DiscountCalculationType.Percentage, 10m,
            new Money(20m), "Line promotion", userId));

        sale.AddDiscount(new SaleDiscount(
            sale.Id, null, DiscountScope.Sale,
            DiscountCalculationType.Percentage, 10m,
            new Money(18m), "Order promotion", userId));

        Assert.Equal(200m, sale.GetSubtotalAmount().Value);
        Assert.Equal(38m, sale.GetDiscountTotalAmount().Value);
        Assert.Equal(162m, sale.GetTotalAmount().Value);
    }

    [Fact]
    public void AddDiscount_WhenAmountExceedsEligibleLineAmount_Throws()
    {
        var (sale, line, userId) = CreateSale(1, 100m);
        var discount = new SaleDiscount(
            sale.Id, line.Id, DiscountScope.SaleLine,
            DiscountCalculationType.FixedAmount, 101m,
            new Money(101m), "Too much", userId);

        Assert.Throws<SaleDomainException>(() => sale.AddDiscount(discount));
    }

    [Fact]
    public void AddLineDiscount_AfterSaleWideDiscount_Throws()
    {
        var (sale, line, userId) = CreateSale(1, 100m);
        sale.AddDiscount(new SaleDiscount(
            sale.Id, null, DiscountScope.Sale,
            DiscountCalculationType.FixedAmount, 10m,
            new Money(10m), "Order discount", userId));

        var lineDiscount = new SaleDiscount(
            sale.Id, line.Id, DiscountScope.SaleLine,
            DiscountCalculationType.FixedAmount, 5m,
            new Money(5m), "Line discount", userId);

        Assert.Throws<SaleDomainException>(() => sale.AddDiscount(lineDiscount));
    }

    [Fact]
    public void ChangeLineQuantity_WhenDiscountExists_Throws()
    {
        var (sale, line, userId) = CreateSale(1, 100m);
        sale.AddDiscount(new SaleDiscount(
            sale.Id, line.Id, DiscountScope.SaleLine,
            DiscountCalculationType.FixedAmount, 10m,
            new Money(10m), "Line discount", userId));

        Assert.Throws<SaleDomainException>(
            () => sale.ChangeLineQuantity(line.Id, 2));
    }

    [Fact]
    public void RemoveDiscount_RecalculatesSaleTotal()
    {
        var (sale, line, userId) = CreateSale(1, 100m);
        var discount = new SaleDiscount(
            sale.Id, line.Id, DiscountScope.SaleLine,
            DiscountCalculationType.FixedAmount, 25m,
            new Money(25m), "Manual discount", userId);
        sale.AddDiscount(discount);

        sale.RemoveDiscount(discount.Id, userId, "Entered in error");

        Assert.Equal(100m, sale.GetTotalAmount().Value);
        Assert.Single(sale.Discounts);
        Assert.True(discount.IsRemoved);
        Assert.Equal(userId, discount.RemovedByUserId);
        Assert.Equal("Entered in error", discount.RemovalReason);

        // Reversed discounts remain for audit but no longer block editing the sale.
        sale.ChangeLineQuantity(line.Id, 2);
        Assert.Equal(200m, sale.GetSubtotalAmount().Value);
        Assert.Equal(200m, sale.GetTotalAmount().Value);
    }

    private static (Sale Sale, SaleLine Line, Guid UserId) CreateSale(
        decimal quantity,
        decimal unitPrice)
    {
        var sale = new Sale(null, DateTime.UtcNow);
        var line = new SaleLine(
            sale.Id, Guid.NewGuid(), quantity, new Money(unitPrice));
        sale.AddLine(line);

        return (sale, line, Guid.NewGuid());
    }
}
