using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Sales.Queries.GetSales;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;
using WholesalePOS.Infrastructure.Persistence.Repositories;

namespace WholesalePOS.Infrastructure.Tests;

public class SaleRepositoryTests
{
    [Fact]
    [TestDatabase]
    public async Task GetByIdWithLinesAsync_ShouldLoadCustomerLinesProductsAndPayments()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var customer = new Customer("Customer One");
        var product = CreateProduct("Product", 100);

        context.Customers.Add(customer);
        context.Products.Add(product);

        var sale = CreateSale(
            product,
            "SALE-DETAIL",
            DateTime.UtcNow,
            customer.Id);

        sale.Confirm();

        sale.AddPayment(
            new Payment(
                sale.Id,
                PaymentMethod.Cash,
                new Money(100),
                DateTime.UtcNow,
                "payment-001",
                "REF-001"));

        context.Sales.Add(sale);

        await context.SaveChangesAsync();

        var repository = new SaleRepository(context);

        var result = await repository.GetByIdWithLinesAsync(
            sale.Id,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Customer);
        Assert.Equal("Customer One", result.Customer!.Name);

        var line = Assert.Single(result.Lines);
        Assert.NotNull(line.Product);
        Assert.Equal("Product", line.Product!.Name);

        var payment = Assert.Single(result.Payments);
        Assert.Equal(PaymentMethod.Cash, payment.Method);
        Assert.Equal("REF-001", payment.ReferenceNumber);
    }

    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldReturnPagedSalesWithCustomerAndCalculatedTotal()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var customer = new Customer("Customer One");
        var productA = CreateProduct("Product A", 100);
        var productB = CreateProduct("Product B", 50);

        context.Customers.Add(customer);
        context.Products.AddRange(productA, productB);

        var sale1 = new Sale(
            customer.Id,
            DateTime.UtcNow.AddMinutes(-1),
            "SALE-001");

        sale1.AddLine(
            new SaleLine(
                sale1.Id,
                productA.Id,
                2,
                new Money(100)));

        sale1.AddLine(
            new SaleLine(
                sale1.Id,
                productB.Id,
                3,
                new Money(50)));

        var sale2 = new Sale(
            null,
            DateTime.UtcNow.AddMinutes(-2),
            "SALE-002");

        sale2.AddLine(
            new SaleLine(
                sale2.Id,
                productA.Id,
                1,
                new Money(100)));

        context.Sales.AddRange(sale1, sale2);

        await context.SaveChangesAsync();

        var repository = new SaleRepository(context);

        var result = await repository.GetPagedAsync(
            new GetSalesQuery
            {
                PageNumber = 1,
                PageSize = 10
            },
            CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);

        var first = result.Items[0];

        Assert.Equal(sale1.Id, first.Id);
        Assert.Equal(customer.Id, first.CustomerId);
        Assert.Equal("Customer One", first.CustomerName);
        Assert.Equal("SALE-001", first.ReferenceNumber);
        Assert.Equal(SaleStatus.Draft, first.Status);
        Assert.Equal(350, first.TotalAmount);
    }

    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldFilterOpenSales()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var product = CreateProduct("Product", 100);
        context.Products.Add(product);

        var draft = CreateSale(product, "DRAFT", DateTime.UtcNow.AddMinutes(-3));
        var confirmed = CreateSale(product, "CONFIRMED", DateTime.UtcNow.AddMinutes(-2));
        confirmed.Confirm();

        var completed = CreateSale(product, "COMPLETED", DateTime.UtcNow.AddMinutes(-1));
        completed.Confirm();
        completed.Complete();

        context.Sales.AddRange(draft, confirmed, completed);

        await context.SaveChangesAsync();

        var repository = new SaleRepository(context);

        var result = await repository.GetPagedAsync(
            new GetSalesQuery
            {
                OpenOnly = true,
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.All(
            result.Items,
            x => Assert.Contains(
                x.Status,
                new[] { SaleStatus.Draft, SaleStatus.Confirmed }));
    }

    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldFilterByStatus()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var product = CreateProduct("Product", 100);
        context.Products.Add(product);

        var draft = CreateSale(product, "DRAFT", DateTime.UtcNow.AddMinutes(-3));
        var confirmed = CreateSale(product, "CONFIRMED", DateTime.UtcNow.AddMinutes(-2));
        confirmed.Confirm();

        context.Sales.AddRange(draft, confirmed);

        await context.SaveChangesAsync();

        var repository = new SaleRepository(context);

        var result = await repository.GetPagedAsync(
            new GetSalesQuery
            {
                Status = SaleStatus.Confirmed,
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(confirmed.Id, item.Id);
        Assert.Equal(SaleStatus.Confirmed, item.Status);
    }

    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldSearchReferenceNumberOrCustomerName()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var customer = new Customer("Maria Store");
        var product = CreateProduct("Product", 100);

        context.Customers.Add(customer);
        context.Products.Add(product);

        var referenceMatch = CreateSale(
            product,
            "REF-MATCH",
            DateTime.UtcNow.AddMinutes(-2),
            customer.Id);

        var customerMatch = CreateSale(
            product,
            "OTHER",
            DateTime.UtcNow.AddMinutes(-1),
            customer.Id);

        context.Sales.AddRange(referenceMatch, customerMatch);

        await context.SaveChangesAsync();

        var repository = new SaleRepository(context);

        var referenceResult = await repository.GetPagedAsync(
            new GetSalesQuery
            {
                Search = "REF-MATCH",
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        var customerResult = await repository.GetPagedAsync(
            new GetSalesQuery
            {
                Search = "Maria",
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        Assert.Single(referenceResult.Items);
        Assert.Equal(referenceMatch.Id, referenceResult.Items[0].Id);

        Assert.Equal(2, customerResult.TotalCount);
    }

    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldApplyPagination()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var product = CreateProduct("Product", 100);
        context.Products.Add(product);

        for (var i = 1; i <= 5; i++)
        {
            var sale = CreateSale(
                product,
                $"SALE-{i:000}",
                DateTime.UtcNow.AddMinutes(-i));

            context.Sales.Add(sale);
        }

        await context.SaveChangesAsync();

        var repository = new SaleRepository(context);

        var result = await repository.GetPagedAsync(
            new GetSalesQuery
            {
                PageNumber = 2,
                PageSize = 2
            },
            CancellationToken.None);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalPages);

        Assert.Equal("SALE-003", result.Items[0].ReferenceNumber);
        Assert.Equal("SALE-004", result.Items[1].ReferenceNumber);
    }

    private static Sale CreateSale(
        Product product,
        string referenceNumber,
        DateTime occurredAt,
        Guid? customerId = null)
    {
        var sale = new Sale(
            customerId,
            occurredAt,
            referenceNumber);

        sale.AddLine(
            new SaleLine(
                sale.Id,
                product.Id,
                1,
                product.DefaultSellingPrice));

        return sale;
    }

    private static Product CreateProduct(
        string name,
        decimal sellingPrice)
    {
        return new Product(
            name,
            null,
            new Money(sellingPrice + 10),
            new Money(sellingPrice));
    }
}
