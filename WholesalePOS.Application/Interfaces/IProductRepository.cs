using System;
using System.Collections.Generic;
using System.Text;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Products.Queries.GetProductById;
using WholesalePOS.Application.Products.Queries.GetProducts;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Specifications;

namespace WholesalePOS.Application.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {

        Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken);

        Task<bool> ExistsByBarcodeAsync(string barcode, CancellationToken cancellationToken);

        Task<PagedResult<ProductListItemDto>> GetPagedAsync(GetProductsQuery query, CancellationToken cancellationToken);

        Task<List<Product>> GetBySpecificationAsync(ISpecification<Product> specification, CancellationToken cancellationToken);

    }
}
