using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Suppliers.Queries.GetSupplierById;

public class GetSupplierByIdHandler
    : IRequestHandler<GetSupplierByIdQuery, SupplierDto>
{
    private readonly ISupplierRepository _supplierRepository;

    public GetSupplierByIdHandler(
        ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<SupplierDto> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (supplier is null)
        {
            throw SupplierErrors.NotFound(request.Id);
        }

        return new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactNumber = supplier.ContactNumber,
            Address = supplier.Address,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt
        };
    }
}