using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Sales.Commands.CheckoutSale;

public sealed class CheckoutSaleHandler
    : IRequestHandler<CheckoutSaleCommand>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IInventoryBalanceRepository _inventoryBalanceRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CheckoutSaleHandler(
        ISaleRepository saleRepository,
        IInventoryBalanceRepository inventoryBalanceRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _inventoryBalanceRepository = inventoryBalanceRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CheckoutSaleCommand request,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdWithLinesAsync(
            request.SaleId,
            cancellationToken);

        if (sale is null)
            throw SaleErrors.NotFound(request.SaleId);

        if (sale.Status != SaleStatus.Confirmed &&
            sale.Status != SaleStatus.Completed)
        {
            throw new InvalidOperationException(
                "Only a confirmed sale can be checked out.");
        }

        if (sale.Status != SaleStatus.Confirmed &&
            sale.Status != SaleStatus.Completed)
        {
            throw new InvalidOperationException(
                "Only a confirmed sale can be checked out.");
        }

        var checkoutKey = request.IdempotencyKey.Trim();

        if (sale.Status == SaleStatus.Completed)
        {
            if (string.Equals(
                    sale.CheckoutIdempotencyKey,
                    checkoutKey,
                    StringComparison.Ordinal))
            {
                return;
            }

            throw CheckoutErrors.AlreadyProcessed(sale.Id);
        }

        if (sale.CheckoutIdempotencyKey is not null &&
            !string.Equals(
                sale.CheckoutIdempotencyKey,
                checkoutKey,
                StringComparison.Ordinal))
        {
            throw CheckoutErrors.IdempotencyKeyConflict();
        }

        var paymentKeys = request.Payments
            .Select(x => x.IdempotencyKey.Trim())
            .ToList();

        if (paymentKeys.Count != paymentKeys.Distinct(
                StringComparer.Ordinal).Count())
        {
            var duplicateKey = paymentKeys
                .GroupBy(x => x, StringComparer.Ordinal)
                .First(x => x.Count() > 1)
                .Key;

            throw CheckoutErrors.DuplicatePaymentIdempotencyKey(
                duplicateKey);
        }

        if (request.Payments.Any(payment =>
                sale.Payments.Any(existing =>
                    string.Equals(
                        existing.IdempotencyKey,
                        payment.IdempotencyKey.Trim(),
                        StringComparison.Ordinal))))
        {
            var duplicateKey = request.Payments
                .Select(x => x.IdempotencyKey.Trim())
                .First(key => sale.Payments.Any(
                    existing => string.Equals(
                        existing.IdempotencyKey,
                        key,
                        StringComparison.Ordinal)));

            throw CheckoutErrors.DuplicatePaymentIdempotencyKey(
                duplicateKey);
        }

        var saleTotal = sale.GetTotalAmount().Value;

        var existingPaymentTotal = sale.Payments.Sum(
            x => x.Amount.Value);

        var requestedPaymentTotal = request.Payments.Sum(
            x => new Money(x.Amount).Value);

        var paymentTotal =
            existingPaymentTotal + requestedPaymentTotal;

        if (paymentTotal != saleTotal)
        {
            throw CheckoutErrors.PaymentTotalMismatch(
                saleTotal,
                paymentTotal);
        }

        foreach (var line in sale.Lines)
        {
            var balance =
                await _inventoryBalanceRepository.GetByProductIdAsync(
                    line.ProductId,
                    cancellationToken);

            if (balance is null)
            {
                if (!request.AllowNegativeInventory)
                {
                    throw CheckoutErrors.InsufficientInventory(
                        line.ProductId);
                }

                if (line.Product.FallbackInventoryCost is null)
                {
                    throw CheckoutErrors.InventoryCostUnavailable(
                        line.ProductId);
                }

                balance = InventoryBalance.CreateEmpty(
                    line.ProductId);

                await _inventoryBalanceRepository.AddAsync(
                    balance,
                    cancellationToken);
            }

            if (line.Quantity > balance.QuantityOnHand &&
                !request.AllowNegativeInventory)
            {
                throw CheckoutErrors.InsufficientInventory(
                    line.ProductId);
            }

            var fallbackCost =
                line.Product.FallbackInventoryCost;

            if (balance.AverageUnitCost.Value <= 0 &&
                fallbackCost is null)
            {
                throw CheckoutErrors.InventoryCostUnavailable(
                    line.ProductId);
            }

            var unitCost = balance.Consume(
                line.Quantity,
                request.AllowNegativeInventory,
                fallbackCost);

            line.SetUnitCost(unitCost);

            var transaction = new InventoryTransaction(
                line.ProductId,
                InventoryTransactionType.Sale,
                InventoryTransactionDirection.Decrease,
                new InventoryTransactionQuantity(line.Quantity),
                unitCost,
                referenceType: "Sale",
                referenceId: sale.Id);

            await _inventoryTransactionRepository.AddAsync(
                transaction,
                cancellationToken);
        }

        foreach (var paymentRequest in request.Payments)
        {
            var payment = new Payment(
                sale.Id,
                paymentRequest.Method,
                new Money(paymentRequest.Amount),
                DateTime.UtcNow,
                paymentRequest.IdempotencyKey,
                paymentRequest.ReferenceNumber);

            sale.AddPayment(payment);
        }

        sale.Complete();
        sale.SetCheckoutIdempotencyKey(checkoutKey);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
