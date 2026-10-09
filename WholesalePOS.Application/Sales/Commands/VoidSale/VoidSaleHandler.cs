using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Sales.Commands.VoidSale;

public sealed class VoidSaleHandler : IRequestHandler<VoidSaleCommand>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public VoidSaleHandler(
        ISaleRepository saleRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        VoidSaleCommand request,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdWithLinesAsync(
            request.SaleId,
            cancellationToken);

        if (sale is null)
            throw SaleErrors.NotFound(request.SaleId);

        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is not Guid userId ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedException(
                "An authenticated employee is required to void a sale.");
        }

        sale.Void(userId, request.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
