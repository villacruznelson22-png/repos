using MediatR;

namespace WholesalePOS.Application.Sales.Commands.CreateSale;

public record CreateSaleCommand(
    Guid? CustomerId,
    DateTime OccurredAt,
    string? ReferenceNumber,
    string? Notes,
    IReadOnlyCollection<CreateSaleLineRequest> Lines
) : IRequest<Guid>;

public record CreateSaleLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitSellingPrice
);  