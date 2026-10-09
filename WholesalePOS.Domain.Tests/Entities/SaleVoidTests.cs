using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests.Entities;

public class SaleVoidTests
{
    [Fact]
    public void Void_CompletedSale_RecordsAuditInformation()
    {
        var sale = CreateCompletedSale();
        var userId = Guid.NewGuid();

        sale.Void(userId, "  Customer transaction entered twice  ");

        Assert.Equal(SaleStatus.Voided, sale.Status);
        Assert.NotNull(sale.VoidedAt);
        Assert.Equal(userId, sale.VoidedByUserId);
        Assert.Equal("Customer transaction entered twice", sale.VoidReason);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Void_WithoutReason_Throws(string reason)
    {
        var sale = CreateCompletedSale();

        Assert.Throws<SaleDomainException>(
            () => sale.Void(Guid.NewGuid(), reason));

        Assert.Equal(SaleStatus.Completed, sale.Status);
        Assert.Null(sale.VoidedAt);
    }

    [Fact]
    public void Void_WithEmptyUserId_Throws()
    {
        var sale = CreateCompletedSale();

        Assert.Throws<SaleDomainException>(
            () => sale.Void(Guid.Empty, "Entered in error"));

        Assert.Equal(SaleStatus.Completed, sale.Status);
    }

    [Fact]
    public void Void_ReasonLongerThan500Characters_Throws()
    {
        var sale = CreateCompletedSale();

        Assert.Throws<SaleDomainException>(
            () => sale.Void(Guid.NewGuid(), new string('x', 501)));

        Assert.Equal(SaleStatus.Completed, sale.Status);
    }

    [Fact]
    public void Void_NonCompletedSale_Throws()
    {
        var sale = new Sale(null, DateTime.UtcNow);

        Assert.Throws<SaleDomainException>(
            () => sale.Void(Guid.NewGuid(), "Entered in error"));

        Assert.Equal(SaleStatus.Draft, sale.Status);
    }

    private static Sale CreateCompletedSale()
    {
        var sale = new Sale(null, DateTime.UtcNow);
        sale.AddLine(new SaleLine(
            sale.Id,
            Guid.NewGuid(),
            quantity: 1,
            unitSellingPrice: new Money(100m)));
        sale.Confirm();
        sale.Complete();
        return sale;
    }
}
