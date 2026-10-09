using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Sales.Commands.RemoveSaleDiscount;

public sealed class RemoveSaleDiscountHandler
    : IRequestHandler<RemoveSaleDiscountCommand>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveSaleDiscountHandler(
        ISaleRepository saleRepository,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemoveSaleDiscountCommand request,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdWithLinesAsync(
            request.SaleId, cancellationToken);

        if (sale is null)
            throw SaleErrors.NotFound(request.SaleId);

        sale.RemoveDiscount(request.DiscountId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
