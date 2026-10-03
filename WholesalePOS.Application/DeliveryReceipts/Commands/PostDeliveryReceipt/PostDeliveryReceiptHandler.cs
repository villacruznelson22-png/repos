using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Services;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.DeliveryReceipts.Commands.PostDeliveryReceipt;

/// <summary>
/// Posts a Delivery Receipt and applies its lines to inventory.
/// This is the application-layer orchestration between the purchasing
/// document and the inventory domain.
/// </summary>
public class PostDeliveryReceiptHandler
    : IRequestHandler<PostDeliveryReceiptCommand>
{
    /// <summary>Loads and updates the Delivery Receipt aggregate.</summary>
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;

    /// <summary>Loads or creates the current balance for each product.</summary>
    private readonly IInventoryBalanceRepository _inventoryBalanceRepository;

    /// <summary>Persists the inventory audit transactions.</summary>
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;

    /// <summary>Applies inventory business operations such as MWAC calculation.</summary>
    private readonly InventoryService _inventoryService;

    /// <summary>Commits the receipt and inventory changes through one UnitOfWork.</summary>
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

    /// <summary>
    /// Posts the receipt and applies every receipt line to inventory.
    /// </summary>
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

        // BUSINESS RULE: DeliveryReceipt.Post() owns the receipt lifecycle.
        // It prevents invalid/empty/already-posted receipts before inventory
        // is touched.
        deliveryReceipt.Post();

        // A DR may contain the same product on multiple lines.
        // Keeping balances in memory lets those lines update the same aggregate
        // sequentially without repeatedly querying the database.
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
                    // First receipt for this product: establish its current-state
                    // record before applying the incoming quantity and cost.
                    balance = InventoryBalance.CreateEmpty(line.ProductId);

                    await _inventoryBalanceRepository.AddAsync(
                        balance,
                        cancellationToken);
                }

                balances.Add(line.ProductId, balance);
            }

            // The DR line is the source of truth for incoming inventory cost.
            // InventoryService updates MWAC and creates the Purchase ledger entry.
            var transaction = _inventoryService.Receive(
                balance,
                line.Quantity,
                new InventoryCost(line.UnitCost.Value),
                "DeliveryReceipt",
                deliveryReceipt.Id);

            await _inventoryTransactionRepository.AddAsync(
                transaction,
                cancellationToken);
        }

        // BUSINESS RULE / ATOMICITY:
        // Receipt status, inventory balances, and inventory transactions are
        // committed through the same UnitOfWork save operation.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
