using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Services;

namespace WholesalePOS.Application.Products.Commands.ReceiveStock;

public class ReceiveStockHandler
    : IRequestHandler<ReceiveStockCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly InventoryService _inventoryService;

    public ReceiveStockHandler(
        IProductRepository productRepository,
        IStockMovementRepository stockMovementRepository,
        IUnitOfWork unitOfWork,
        InventoryService inventoryService)
    {
        _productRepository = productRepository;
        _stockMovementRepository = stockMovementRepository;
        _unitOfWork = unitOfWork;
        _inventoryService = inventoryService;
    }

    public async Task Handle(
        ReceiveStockCommand request,
        CancellationToken cancellationToken)
    {
        var product =
            await _productRepository.GetByIdAsync(
                request.ProductId,
                cancellationToken);

        if (product is null)
            throw new NotFoundException(
                "Product not found.");

        var movement =
            _inventoryService.ReceiveStock(
                product,
                request.Quantity);

        await _stockMovementRepository.AddAsync(
            movement,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}