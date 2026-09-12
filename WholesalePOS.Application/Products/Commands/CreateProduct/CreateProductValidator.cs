    using FluentValidation;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace WholesalePOS.Application.Products.Commands.CreateProduct
    {
        public class CreateProductValidator : AbstractValidator<CreateProductCommand>
        {
            public CreateProductValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .MaximumLength(200);

                RuleFor(x => x.DefaultSellingPrice)
                    .GreaterThanOrEqualTo(0);

                RuleFor(x => x.SuggestedRetailPrice)
                    .GreaterThanOrEqualTo(0);

                RuleFor(x => x.Barcode)
                    .MaximumLength(100);
            }
        }
    }
