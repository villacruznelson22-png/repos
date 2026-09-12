using System.Linq.Expressions;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Specifications.Products;

public class MinimumSellingPriceSpecification : ISpecification<Product>
{
    private readonly Money _minimumPrice;

    public MinimumSellingPriceSpecification(Money minimumPrice)
    {
        _minimumPrice = minimumPrice;
    }

    public Expression<Func<Product, bool>> Criteria =>
         product => product.DefaultSellingPrice >= _minimumPrice;
}