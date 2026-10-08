using WholesalePOS.Domain.Entities;
using WholesalePOS.Infrastructure.Persistence.Repositories;

namespace WholesalePOS.Infrastructure.Tests;

public class CustomerRepositoryTests
{
    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldSearchByName()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var juan = new Customer("Juan Dela Cruz", "09171234567");
        var maria = new Customer("Maria Santos", "09181234567");

        context.Customers.AddRange(juan, maria);
        await context.SaveChangesAsync();

        var repository = new CustomerRepository(context);

        var result = await repository.GetPagedAsync(
            new WholesalePOS.Application.Customers.Queries.GetCustomers.GetCustomersQuery
            {
                Search = "juan",
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(juan.Id, item.Id);
        Assert.Equal("Juan Dela Cruz", item.Name);
    }

    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldSearchByContactNumber()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var customer = new Customer("Juan Dela Cruz", "09171234567");
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var repository = new CustomerRepository(context);

        var result = await repository.GetPagedAsync(
            new WholesalePOS.Application.Customers.Queries.GetCustomers.GetCustomersQuery
            {
                Search = "1234567",
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(customer.Id, item.Id);
        Assert.Equal("Juan Dela Cruz", item.Name);
    }

    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldFilterActiveCustomers()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        var active = new Customer("Active Customer");
        var inactive = new Customer("Inactive Customer");
        inactive.Deactivate();

        context.Customers.AddRange(active, inactive);
        await context.SaveChangesAsync();

        var repository = new CustomerRepository(context);

        var result = await repository.GetPagedAsync(
            new WholesalePOS.Application.Customers.Queries.GetCustomers.GetCustomersQuery
            {
                ActiveOnly = true,
                PageNumber = 1,
                PageSize = 20
            },
            CancellationToken.None);

        var item = Assert.Single(result.Items);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(active.Id, item.Id);
        Assert.True(item.IsActive);
    }

    [Fact]
    [TestDatabase]
    public async Task GetPagedAsync_ShouldApplyPagination()
    {
        var factory = new TestDbContextFactory();

        await using var context = factory.Create();

        for (var i = 1; i <= 5; i++)
        {
            context.Customers.Add(
                new Customer($"Customer {i:00}"));
        }

        await context.SaveChangesAsync();

        var repository = new CustomerRepository(context);

        var result = await repository.GetPagedAsync(
            new WholesalePOS.Application.Customers.Queries.GetCustomers.GetCustomersQuery
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
        Assert.Equal("Customer 03", result.Items[0].Name);
        Assert.Equal("Customer 04", result.Items[1].Name);
    }
}
