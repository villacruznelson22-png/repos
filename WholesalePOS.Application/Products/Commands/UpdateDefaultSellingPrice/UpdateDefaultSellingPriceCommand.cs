using MediatR;

namespace WholesalePOS.Application.Products.Commands.UpdateSellingPrice;

public class UpdateDefaultSellingPriceCommand : IRequest
{
    public Guid ProductId { get; set; }

    public decimal NewDefaultSellingPrice { get; set; }
}