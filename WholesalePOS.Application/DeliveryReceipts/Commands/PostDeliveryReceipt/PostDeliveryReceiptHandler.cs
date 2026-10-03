using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Services;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.DeliveryReceipts.Commands.PostDeliveryReceipt;

public class PostDeliveryReceiptHandler
    : IRequestHandler<PostDeliveryReceiptCommand>
{
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    private readonly IInventoryBalanceRepository _inventoryBalanceRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly InventoryService _inventoryService;
    private readonly IUnitOfWork _unitOfWork;

    public PostDeliveryReceiptHandler(
        IDeliveryReceiptRepository deliveryReceiptRepository,
        IInventoryBalanceRepository inventoryBalanceRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        InventoryService inventoryService,
        IUnitOfWork unitOfWork)
    {
        _deliveryReceiptRepository = deliveryReceiptRepository;
        _inventoryBalanceRepository = inventoryBalanceRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _inventoryService = inventoryService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        PostDeliveryReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var deliveryReceipt =
            await _deliveryReceiptRepository.GetByIdWithLinesAsync(
                request.Id,
                cancellationToken);

        if (deliveryReceipt is null)
        {
            throw new NotFoundException(
                $"Delivery receipt '{request.Id}' was not found.");
        }

        deliveryReceipt.Post();

        var balances = new Dictionary<Guid, InventoryBalance>();

        foreach (var line in deliveryReceipt.Lines)
        {
            if (!balances.TryGetValue(line.ProductId, out var balance))
            {
                balance =
                    await _inventoryBalanceRepository.GetByProductIdAsync(
                        line.ProductId,
                        cancellationToken);

                if (balance is null)
                {
                    balance = InventoryBalance.CreateEmpty(
                        line.ProductId);

                    await _inventoryBalanceRepository.AddAsync(
                        balance,
                        cancellationToken);
                }

                balances.Add(line.ProductId, balance);
            }

            var transaction = _inventoryService.Receive(
                balance,
                line.Quantity,
                new WholesalePOS.Domain.ValueObjects.InventoryCost(
                    line.UnitCost.Value),
                "DeliveryReceipt",
                deliveryReceipt.Id);

            await _inventoryTransactionRepository.AddAsync(
                transaction,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
