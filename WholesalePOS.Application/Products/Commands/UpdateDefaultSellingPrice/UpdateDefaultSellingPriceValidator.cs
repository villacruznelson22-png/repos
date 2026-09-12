using FluentValidation;

namespace WholesalePOS.Application.Products.Commands.UpdateSellingPrice;

public class UpdateDefaultSellingPriceValidator: AbstractValidator<UpdateDefaultSellingPriceCommand>
{
    public UpdateDefaultSellingPriceValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.NewDefaultSellingPrice)
            .GreaterThanOrEqualTo(0);
    }
}