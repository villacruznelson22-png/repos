using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void Constructor_ShouldSetPrices()
    {
        var srp = new Money(199);
        var defaultSellingPrice = new Money(205);

        var product = new Product(
            "Cobra Astig 290mL",
            new Barcode("123456"),
            srp,
            defaultSellingPrice);

        Assert.Equal(
            srp,
            product.SuggestedRetailPrice);

        Assert.Equal(
            defaultSellingPrice,
            product.DefaultSellingPrice);
    }

    [Fact]
    public void ChangeName_ShouldThrow_WhenNameIsEmpty()
    {
        var product = new Product(
            "Test Product",
            new Barcode("123456"),
            new Money(30),
            new Money(35));

        var action = () =>
            product.ChangeName("");

        Assert.Throws<ProductDomainException>(action);
    }

    [Fact]
    public void ChangeName_ShouldUpdateName_WhenNameIsValid()
    {
        var product = new Product(
            "Old Name",
            new Barcode("123456"),
            new Money(30),
            new Money(35));

        product.ChangeName("New Name");

        Assert.Equal("New Name", product.Name);
    }

    [Fact]
    public void ChangeSuggestedRetailPrice_ShouldThrow_WhenPriceIsNegative()
    {
        var product = new Product(
            "Test Product",
            new Barcode("TEST-001"),
            new Money(20),
            new Money(25));

        var action = () =>
            product.ChangeSuggestedRetailPrice(
                new Money(-10));

        Assert.Throws<ProductDomainException>(action);
    }

    [Fact]
    public void ChangeSuggestedRetailPrice_ShouldUpdatePrice_WhenPriceIsValid()
    {
        var product = new Product(
            "Test Product",
            new Barcode("TEST-001"),
            new Money(20),
            new Money(25));

        product.ChangeSuggestedRetailPrice(
            new Money(30));

        Assert.Equal(
            new Money(30),
            product.SuggestedRetailPrice);
    }

    [Fact]
    public void ChangeDefaultSellingPrice_ShouldThrow_WhenPriceIsNegative()
    {
        var product = new Product(
            "Test Product",
            new Barcode("TEST-001"),
            new Money(20),
            new Money(25));

        var action = () =>
            product.ChangeDefaultSellingPrice(
                new Money(-10));

        Assert.Throws<ProductDomainException>(action);
    }

    [Fact]
    public void ChangeDefaultSellingPrice_ShouldUpdatePrice_WhenPriceIsValid()
    {
        var product = new Product(
            "Test Product",
            new Barcode("TEST-001"),
            new Money(20),
            new Money(25));

        product.ChangeDefaultSellingPrice(
            new Money(30));

        Assert.Equal(
            new Money(30),
            product.DefaultSellingPrice);
    }

    [Fact]
    public void SuggestedRetailPrice_And_DefaultSellingPrice_ShouldBeIndependent()
    {
        var product = new Product(
            "Cobra Astig 290mL",
            new Barcode("123456"),
            new Money(199),
            new Money(205));

        product.ChangeDefaultSellingPrice(
            new Money(210));

        Assert.Equal(
            new Money(199),
            product.SuggestedRetailPrice);

        Assert.Equal(
            new Money(210),
            product.DefaultSellingPrice);
    }

    [Fact]
    public void ChangeBarcode_ShouldUpdateBarcode()
    {
        var product = new Product(
            "Test Product",
            new Barcode("123456"),
            new Money(30),
            new Money(35));

        product.ChangeBarcode(
            new Barcode("789012"));

        Assert.Equal(
            new Barcode("789012"),
            product.Barcode);
    }

    [Fact]
    public void AddStock_ShouldIncreaseStock()
    {
        var product = new Product(
            "Test Product",
            new Barcode("123456"),
            new Money(30),
            new Money(35));

        product.AddStock(100);

        Assert.Equal(
            new StockQuantity(100),
            product.Stock);
    }

    [Fact]
    public void RemoveStock_ShouldDecreaseStock()
    {
        var product = new Product(
            "Test Product",
            new Barcode("123456"),
            new Money(30),
            new Money(35));

        product.AddStock(100);

        product.RemoveStock(25);

        Assert.Equal(
            new StockQuantity(75),
            product.Stock);
    }

    [Fact]
    public void RemoveStock_ShouldSupportFractionalQuantity()
    {
        var product = new Product(
            "Coke Kasalo",
            new Barcode("123456"),
            new Money(280),
            new Money(260));

        product.AddStock(10);

        product.RemoveStock(0.5m);

        Assert.Equal(
            new StockQuantity(9.5m),
            product.Stock);
    }

    [Fact]
    public void RemoveStock_ShouldThrow_WhenInsufficientStock()
    {
        var product = new Product(
            "Test Product",
            new Barcode("123456"),
            new Money(30),
            new Money(35));

        product.AddStock(10);

        var action = () =>
            product.RemoveStock(10.5m);

        Assert.Throws<InvalidOperationException>(action);
    }
}