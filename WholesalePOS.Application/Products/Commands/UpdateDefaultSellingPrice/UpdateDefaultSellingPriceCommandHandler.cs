using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Products.Commands.UpdateSellingPrice;

public class UpdateDefaultSellingPriceCommandHandler : IRequestHandler<UpdateDefaultSellingPriceCommand>
{
    private readonly IRepository<Product> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDefaultSellingPriceCommandHandler(IRepository<Product> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDefaultSellingPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(
            request.ProductId,
            cancellationToken);

        if (product is null)
            throw ProductErrors.NotFound(request.ProductId);

        product.ChangeDefaultSellingPrice(
          new Money(request.NewDefaultSellingPrice));

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);


    }
}