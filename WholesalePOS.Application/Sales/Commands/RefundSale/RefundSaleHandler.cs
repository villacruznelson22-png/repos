using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Application.Sales.Commands.RefundSale;

public sealed class RefundSaleHandler : IRequestHandler<RefundSaleCommand, Guid>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public RefundSaleHandler(
        ISaleRepository saleRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        RefundSaleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is not Guid userId ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedException(
                "An authenticated employee is required to record a refund.");
        }

        if (!Enum.IsDefined(request.Method))
            throw new SaleDomainException("A valid refund payment method is required.");

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new SaleDomainException("Refund idempotency key cannot be empty.");

        // A retry with the same key and payload returns the original result.
        // Reusing a key for a different operation is rejected.
        var existingRefund = await _saleRepository.GetRefundByIdempotencyKeyAsync(
            request.IdempotencyKey,
            cancellationToken);

        if (existingRefund is not null)
        {
            var sameRequest =
                existingRefund.SaleId == request.SaleId &&
                existingRefund.Amount.Value == new Money(request.Amount).Value &&
                existingRefund.Method == request.Method &&
                string.Equals(existingRefund.Reason, request.Reason?.Trim(), StringComparison.Ordinal) &&
                string.Equals(existingRefund.ReferenceNumber, Normalize(request.ReferenceNumber), StringComparison.Ordinal);

            if (!sameRequest)
                throw new SaleDomainException(
                    "This refund idempotency key has already been used for a different request.");

            return existingRefund.Id;
        }

        var sale = await _saleRepository.GetByIdWithLinesAsync(
            request.SaleId,
            cancellationToken);

        if (sale is null)
            throw SaleErrors.NotFound(request.SaleId);

        var refund = new SaleRefund(
            sale.Id,
            new Money(request.Amount),
            request.Method,
            DateTime.UtcNow,
            userId,
            request.Reason,
            request.IdempotencyKey,
            request.ReferenceNumber);

        sale.AddRefund(refund);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return refund.Id;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
