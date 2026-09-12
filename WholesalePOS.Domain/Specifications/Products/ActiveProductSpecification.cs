using System.Linq.Expressions;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Domain.Specifications.Products;

public class ActiveProductSpecification: ISpecification<Product>
{
    public Expression<Func<Product, bool>> Criteria =>
      product => product.IsActive;
}