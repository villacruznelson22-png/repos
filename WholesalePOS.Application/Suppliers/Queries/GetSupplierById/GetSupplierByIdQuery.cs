using MediatR;

namespace WholesalePOS.Application.Suppliers.Queries.GetSupplierById;

public record GetSupplierByIdQuery(
    Guid Id
) : IRequest<SupplierDto>;