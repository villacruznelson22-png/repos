using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Sales.Commands.ApplySaleDiscount;

public sealed class ApplySaleDiscountHandler
    : IRequestHandler<ApplySaleDiscountCommand, Guid>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ApplySaleDiscountHandler(
        ISaleRepository saleRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        ApplySaleDiscountCommand request,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdWithLinesAsync(
            request.SaleId, cancellationToken);

        if (sale is null)
            throw SaleErrors.NotFound(request.SaleId);

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is not Guid userId ||
            userId == Guid.Empty)
            throw new UnauthorizedAccessException(
                "An authenticated employee is required to apply a discount.");

        decimal eligibleAmount;
        if (request.Scope == DiscountScope.SaleLine)
        {
            if (request.SaleLineId is not Guid lineId)
                throw new SaleDomainException(
                    "A sale line is required for a line-item discount.");

            var line = sale.Lines.SingleOrDefault(x => x.Id == lineId);
            if (line is null)
                throw new SaleDomainException(
                    "The selected sale line was not found.");

            eligibleAmount = line.GetTotalAmount().Value -
                sale.Discounts
                    .Where(x => !x.IsRemoved && x.Scope == DiscountScope.SaleLine &&
                                x.SaleLineId == lineId)
                    .Sum(x => x.Amount.Value);
        }
        else if (request.Scope == DiscountScope.Sale)
        {
            eligibleAmount = sale.GetSubtotalAmount().Value -
                sale.Discounts.Where(x => !x.IsRemoved).Sum(x => x.Amount.Value);
        }
        else
        {
            throw new SaleDomainException("Discount scope is invalid.");
        }

        if (eligibleAmount <= 0)
            throw new SaleDomainException(
                "There is no remaining amount available for a discount.");

        var calculatedAmount = request.CalculationType switch
        {
            DiscountCalculationType.Percentage =>
                decimal.Round(eligibleAmount * request.Value / 100m, 2,
                    MidpointRounding.AwayFromZero),
            DiscountCalculationType.FixedAmount =>
                decimal.Round(request.Value, 2, MidpointRounding.AwayFromZero),
            _ => throw new SaleDomainException(
                "Discount calculation type is invalid.")
        };

        if (calculatedAmount <= 0)
            throw new SaleDomainException(
                "Calculated discount must be greater than zero.");

        if (calculatedAmount > eligibleAmount)
            throw new SaleDomainException(
                "Discount cannot exceed the remaining eligible amount.");

        var discount = new SaleDiscount(
            sale.Id,
            request.SaleLineId,
            request.Scope,
            request.CalculationType,
            request.Value,
            new Money(calculatedAmount),
            request.Reason,
            userId);

        sale.AddDiscount(discount);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return discount.Id;
    }
}
