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

    public RefundSaleHandler(
        ISaleRepository saleRepository,
        ICurrentUser currentUser)
    {
        _saleRepository = saleRepository;
        _currentUser = currentUser;
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

        // Build the proposed record; the repository serializes the idempotency
        // check, balance validation, and insert in one database transaction.
        var proposedRefund = new SaleRefund(
            request.SaleId,
            new Money(request.Amount),
            request.Method,
            DateTime.UtcNow,
            userId,
            request.Reason,
            request.IdempotencyKey,
            request.ReferenceNumber);

        var persistedRefund = await _saleRepository.CreateRefundAtomicallyAsync(
            proposedRefund,
            cancellationToken);

        if (persistedRefund.Id != proposedRefund.Id)
        {
            var sameRequest =
                persistedRefund.SaleId == request.SaleId &&
                persistedRefund.Amount.Value == proposedRefund.Amount.Value &&
                persistedRefund.Method == request.Method &&
                string.Equals(persistedRefund.Reason, request.Reason.Trim(), StringComparison.Ordinal) &&
                string.Equals(persistedRefund.ReferenceNumber, Normalize(request.ReferenceNumber), StringComparison.Ordinal);

            if (!sameRequest)
                throw new SaleDomainException(
                    "This refund idempotency key has already been used for a different request.");
        }

        return persistedRefund.Id;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
