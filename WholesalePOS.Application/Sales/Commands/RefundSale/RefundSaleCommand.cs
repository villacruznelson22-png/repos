using MediatR;
using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Application.Sales.Commands.RefundSale;

public sealed record RefundSaleCommand(
    Guid SaleId,
    decimal Amount,
    PaymentMethod Method,
    string Reason,
    string IdempotencyKey,
    string? ReferenceNumber = null) : IRequest<Guid>;
