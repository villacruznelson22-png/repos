using MediatR;

namespace WholesalePOS.Application.Products.Commands.ReceiveStock;

public record ReceiveStockCommand(
    Guid ProductId,
    decimal Quantity) : IRequest;