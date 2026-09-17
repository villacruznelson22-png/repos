using MediatR;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Suppliers.Commands.CreateSupplier;

public class CreateSupplierHandler
    : IRequestHandler<CreateSupplierCommand, Guid>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = new Supplier(
            request.Name,
            request.ContactNumber,
            request.Address,
            request.Notes);

        await _supplierRepository.AddAsync(
            supplier,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return supplier.Id;
    }
}