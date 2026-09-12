using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Products.Queries.GetProducts;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Specifications;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{

    public ProductRepository(WholesalePosDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x => x.Barcode != null && x.Barcode.Value == barcode, cancellationToken);
    }

    public async Task<bool> ExistsByBarcodeAsync(string barcode, CancellationToken cancellationToken)
    {
        return await _context.Products
            .AnyAsync(x => x.Barcode != null && x.Barcode.Value == barcode, cancellationToken);
    }

    public async Task<PagedResult<ProductListItemDto>> GetPagedAsync(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = _dbSet.AsNoTracking();
        var totalCount = await products.CountAsync(cancellationToken);
        var items = await products
                .OrderBy(p => p.Name)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(p => new ProductListItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Barcode = p.Barcode == null ? null : p.Barcode.Value,
                    SellingPrice = p.DefaultSellingPrice.Value
                })
                .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemDto>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<Product>> GetBySpecificationAsync(ISpecification<Product> specification, CancellationToken cancellationToken)
    {
        return await _dbSet
        .Where(specification.Criteria)
        .ToListAsync(cancellationToken);
    }
}