using Microsoft.EntityFrameworkCore;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Application.Products.Queries.GetProducts;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Specifications;
using WholesalePOS.Domain.ValueObjects;
using WholesalePOS.Infrastructure.Persistence;

namespace WholesalePOS.Infrastructure.Persistence.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(WholesalePosDbContext context)
        : base(context)
    {
    }

    public async Task<Product?> GetByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken)
    {
        // BUSINESS RULE: Barcode uniqueness is enforced against the persisted Barcode value.
        // Comparing the value object itself allows EF Core to apply the configured value converter
        // instead of trying to translate the nested Barcode.Value property.
        var productBarcode = new Barcode(barcode);

        return await _context.Products
            .FirstOrDefaultAsync(
                x => x.Barcode == productBarcode,
                cancellationToken);
    }

    public async Task<bool> ExistsByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken)
    {
        // BUSINESS RULE: A barcode may belong to only one product.
        // Compare the Barcode value object so EF Core can translate the configured conversion to SQL.
        var productBarcode = new Barcode(barcode);

        return await _context.Products
            .AnyAsync(
                x => x.Barcode == productBarcode,
                cancellationToken);
    }

    public async Task<PagedResult<ProductListItemDto>> GetPagedAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = _dbSet.AsNoTracking();

        if (query.ActiveOnly)
        {
            products = products.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            products = products.Where(p =>
                p.Name.Contains(search));
        }

        var totalCount = await products.CountAsync(
            cancellationToken);

        var items = await products
            .OrderBy(p => p.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductListItemDto
            {
                Id = p.Id,
                Name = p.Name,
                Barcode = p.Barcode == null
                    ? null
                    : p.Barcode.Value,
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

    public async Task<List<Product>> GetBySpecificationAsync(
        ISpecification<Product> specification,
        CancellationToken cancellationToken)
    {
        return await _dbSet
            .Where(specification.Criteria)
            .ToListAsync(cancellationToken);
    }
}
