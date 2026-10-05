using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Sales.Commands.ConfirmSale;

public sealed class ConfirmSaleHandler
    : IRequestHandler<ConfirmSaleCommand>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmSaleHandler(
        ISaleRepository saleRepository,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ConfirmSaleCommand request,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdWithLinesAsync(
            request.SaleId,
            cancellationToken);

        if (sale is null)
            throw SaleErrors.NotFound(request.SaleId);

        // Domain is responsible for validating whether
        // the sale is allowed to move from Draft to Confirmed.
        sale.Confirm();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}