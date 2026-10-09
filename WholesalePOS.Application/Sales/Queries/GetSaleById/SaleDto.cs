using WholesalePOS.Domain.Enums;

namespace WholesalePOS.Application.Sales.Queries.GetSaleById;

public sealed class SaleDto
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public SaleStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public Guid? VoidedByUserId { get; set; }
    public string? VoidReason { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountTotalAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalRefundedAmount { get; set; }
    public decimal RemainingRefundableAmount { get; set; }
    public List<SaleLineDto> Lines { get; set; } = new();
    public List<SalePaymentDto> Payments { get; set; } = new();
    public List<SaleDiscountDto> Discounts { get; set; } = new();
    public List<SaleRefundDto> Refunds { get; set; } = new();
}

public sealed class SaleLineDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitSellingPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class SalePaymentDto
{
    public Guid Id { get; set; }
    public int Method { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public string? ReferenceNumber { get; set; }
}

public sealed class SaleDiscountDto
{
    public Guid Id { get; set; }
    public Guid? SaleLineId { get; set; }
    public int Scope { get; set; }
    public int CalculationType { get; set; }
    public decimal Value { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid AppliedByUserId { get; set; }
    public DateTime AppliedAt { get; set; }
    public bool IsRemoved { get; set; }
    public DateTime? RemovedAt { get; set; }
    public Guid? RemovedByUserId { get; set; }
    public string? RemovalReason { get; set; }
}

public sealed class SaleRefundDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public int Method { get; set; }
    public DateTime RefundedAt { get; set; }
    public Guid RefundedByUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
}