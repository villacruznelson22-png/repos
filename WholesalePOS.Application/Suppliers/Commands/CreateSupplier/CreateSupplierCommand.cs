using MediatR;

namespace WholesalePOS.Application.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand(
    string Name,
    string? ContactNumber,
    string? Address,
    string? Notes
) : IRequest<Guid>;