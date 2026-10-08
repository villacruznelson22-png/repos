using MediatR;

namespace WholesalePOS.Application.Sales.Commands.UpdateSale;

public sealed record UpdateSaleCommand(
    Guid SaleId,
    Guid? CustomerId,
    DateTime OccurredAt,
    string? ReferenceNumber,
    string? Notes,
    IReadOnlyCollection<UpdateSaleLineRequest> Lines
) : IRequest;

public sealed record UpdateSaleLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitSellingPrice
);
