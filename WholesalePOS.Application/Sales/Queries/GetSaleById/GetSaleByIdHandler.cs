using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Sales.Queries.GetSaleById;

public sealed class GetSaleByIdHandler
    : IRequestHandler<GetSaleByIdQuery, SaleDto>
{
    private readonly ISaleRepository _saleRepository;

    public GetSaleByIdHandler(
        ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<SaleDto> Handle(
        GetSaleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdWithLinesAsync(
            request.SaleId,
            cancellationToken);

        if (sale is null)
            throw SaleErrors.NotFound(request.SaleId);

        return new SaleDto
        {
            Id = sale.Id,
            CustomerId = sale.CustomerId,
            OccurredAt = sale.OccurredAt,
            ReferenceNumber = sale.ReferenceNumber,
            Notes = sale.Notes,
            Status = sale.Status,
            CreatedAt = sale.CreatedAt,
            ConfirmedAt = sale.ConfirmedAt,
            CompletedAt = sale.CompletedAt,
            CancelledAt = sale.CancelledAt,
            VoidedAt = sale.VoidedAt,
            SubtotalAmount = sale.GetSubtotalAmount().Value,
            DiscountTotalAmount = sale.GetDiscountTotalAmount().Value,
            TotalAmount = sale.GetTotalAmount().Value,

            Lines = sale.Lines
                .Select(line => new SaleLineDto
                {
                    Id = line.Id,
                    ProductId = line.ProductId,
                    ProductName = line.Product.Name,
                    Quantity = line.Quantity,
                    UnitSellingPrice = line.UnitSellingPrice.Value,
                    LineTotal = line.GetTotalAmount().Value
                })
                .ToList(),

            Payments = sale.Payments
                .Select(payment => new SalePaymentDto
                {
                    Id = payment.Id,
                    Method = (int)payment.Method,
                    Amount = payment.Amount.Value,
                    PaidAt = payment.PaidAt,
                    ReferenceNumber = payment.ReferenceNumber
                })
                .ToList(),

            Discounts = sale.Discounts
                .Select(discount => new SaleDiscountDto
                {
                    Id = discount.Id,
                    SaleLineId = discount.SaleLineId,
                    Scope = (int)discount.Scope,
                    CalculationType = (int)discount.CalculationType,
                    Value = discount.Value,
                    Amount = discount.Amount.Value,
                    Reason = discount.Reason,
                    AppliedByUserId = discount.AppliedByUserId,
                    AppliedAt = discount.AppliedAt,
                    IsRemoved = discount.IsRemoved,
                    RemovedAt = discount.RemovedAt,
                    RemovedByUserId = discount.RemovedByUserId,
                    RemovalReason = discount.RemovalReason
                })
                .ToList()
        };
    }
}
