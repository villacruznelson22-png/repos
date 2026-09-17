using MediatR;

namespace WholesalePOS.Application.Suppliers.Commands.ActivateSupplier;

public record ActivateSupplierCommand(
    Guid Id
) : IRequest;