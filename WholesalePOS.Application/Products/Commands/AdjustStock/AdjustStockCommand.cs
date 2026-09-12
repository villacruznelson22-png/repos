using MediatR;

namespace WholesalePOS.Application.Products.Commands.AdjustStock;

public record AdjustStockCommand(
    Guid ProductId,
    decimal Quantity) : IRequest;