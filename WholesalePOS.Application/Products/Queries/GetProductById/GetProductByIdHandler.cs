using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Specifications.Products;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Products.Queries.GetProductById
{
    public class GetProductByIdHandler
       : IRequestHandler<GetProductByIdQuery, ProductDto>
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDto> Handle(
            GetProductByIdQuery request,
            CancellationToken cancellationToken)
        {

            var product = await _productRepository.GetByIdAsync(
                    request.Id,
                    cancellationToken);

            if (product is null)
                throw ProductErrors.NotFound(request.Id);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Barcode = product.Barcode?.Value,
                SellingPrice = product.DefaultSellingPrice.Value
            };
        }
    }
}
