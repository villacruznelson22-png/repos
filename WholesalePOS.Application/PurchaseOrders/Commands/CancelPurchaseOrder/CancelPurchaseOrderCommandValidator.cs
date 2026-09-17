using FluentValidation;

namespace WholesalePOS.Application.PurchaseOrders.Commands.CancelPurchaseOrder;

public class CancelPurchaseOrderCommandValidator
    : AbstractValidator<CancelPurchaseOrderCommand>
{
    public CancelPurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}