using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Products.Commands.CreateProduct
{
    public class CreateProductHandler
    : IRequestHandler<CreateProductCommand, Guid>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(
            CreateProductCommand request,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.Barcode))
            {
                var exists = await _productRepository.ExistsByBarcodeAsync(
                    request.Barcode,
                    cancellationToken);

                if (exists)
                    throw ProductErrors.DuplicateBarcode(request.Barcode!);
            }

            var barcode = string.IsNullOrWhiteSpace(request.Barcode)? null: new Barcode(request.Barcode);

            var product = new Product(request.Name,
                                    barcode,
                                    new Money(request.SuggestedRetailPrice),
                                    new Money(request.DefaultSellingPrice)
                                    );

            await _productRepository.AddAsync(product, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
    }
}
