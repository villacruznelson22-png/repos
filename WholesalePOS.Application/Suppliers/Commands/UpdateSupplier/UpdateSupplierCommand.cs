using MediatR;

namespace WholesalePOS.Application.Suppliers.Commands.UpdateSupplier;

public record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string? ContactNumber,
    string? Address,
    string? Notes
) : IRequest;