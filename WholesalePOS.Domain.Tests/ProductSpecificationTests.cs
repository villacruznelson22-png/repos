using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Specifications;
using WholesalePOS.Domain.Specifications.Products;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests;

public class ProductSpecificationTests
{
    [Fact]
    public void ShouldBeSatisfied_WhenDefaultSellingPriceMeetsMinimum()
    {
        var product = new Product(
            "Test Product",
            new Barcode("123456"),
            new Money(100),
            new Money(150));

        var specification =
            new MinimumSellingPriceSpecification(
                new Money(100));

        Assert.True(
            specification.Criteria.Compile()(product));
    }

    [Fact]
    public void ShouldNotBeSatisfied_WhenDefaultSellingPriceIsBelowMinimum()
    {
        var product = new Product(
            "Test Product",
            new Barcode("123456"),
            new Money(40),
            new Money(50));

        var specification =
            new MinimumSellingPriceSpecification(
                new Money(100));

        Assert.False(
            specification.Criteria.Compile()(product));
    }

    [Fact]
    public void ShouldBeSatisfied_WhenAllSpecificationsAreSatisfied()
    {
        var product = new Product(
            "Test Product",
            new Barcode("123456"),
            new Money(100),
            new Money(150));

        var minimumPrice =
            new MinimumSellingPriceSpecification(
                new Money(100));

        var hasBarcode =
            new HasBarcodeSpecification();

        var specification =
            new AndSpecification<Product>(
                minimumPrice,
                hasBarcode);

        Assert.True(
            specification.Criteria.Compile()(product));
    }

    [Fact]
    public void ShouldNotBeSatisfied_WhenOneSpecificationFails()
    {
        var product = new Product(
            "Test Product",
            null,
            new Money(100),
            new Money(150));

        var minimumPrice =
            new MinimumSellingPriceSpecification(
                new Money(100));

        var hasBarcode =
            new HasBarcodeSpecification();

        var specification =
            new AndSpecification<Product>(
                minimumPrice,
                hasBarcode);

        Assert.False(
            specification.Criteria.Compile()(product));
    }
}