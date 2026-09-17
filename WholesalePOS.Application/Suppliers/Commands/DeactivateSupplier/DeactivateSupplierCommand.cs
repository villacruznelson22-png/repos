using MediatR;

namespace WholesalePOS.Application.Suppliers.Commands.DeactivateSupplier;

public record DeactivateSupplierCommand(
    Guid Id
) : IRequest;