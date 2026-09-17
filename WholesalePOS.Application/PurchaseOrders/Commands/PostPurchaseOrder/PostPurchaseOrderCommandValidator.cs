using FluentValidation;

namespace WholesalePOS.Application.PurchaseOrders.Commands.PostPurchaseOrder;

public class PostPurchaseOrderCommandValidator
    : AbstractValidator<PostPurchaseOrderCommand>
{
    public PostPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}