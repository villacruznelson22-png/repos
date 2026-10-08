using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Application.Sales.Queries.GetSales;

public sealed class SaleListItemDto
{
    public Guid Id { get; init; }

    public Guid? CustomerId { get; init; }

    public string? CustomerName { get; init; }

    public DateTime OccurredAt { get; init; }

    public string? ReferenceNumber { get; init; }

    public SaleStatus Status { get; init; }

    public decimal TotalAmount { get; init; }
}