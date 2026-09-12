using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Products.Commands.AdjustStock;

public class AdjustStockHandler
    : IRequestHandler<AdjustStockCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdjustStockHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var product =
            await _productRepository.GetByIdAsync(
                request.ProductId,
                cancellationToken);

        if (product is null)
            throw ProductErrors.NotFound(request.ProductId);

        if (request.Quantity >= 0)
        {
            product.AddStock(request.Quantity);
        }
        else
        {
            product.RemoveStock(
                Math.Abs(request.Quantity));
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}