using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Suppliers.Commands.UpdateSupplier;

public class UpdateSupplierHandler
    : IRequestHandler<UpdateSupplierCommand>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (supplier is null)
        {
            throw SupplierErrors.NotFound(request.Id);
        }

        supplier.ChangeName(request.Name);
        supplier.ChangeContactNumber(request.ContactNumber);
        supplier.ChangeAddress(request.Address);
        supplier.ChangeNotes(request.Notes);

        _supplierRepository.Update(supplier);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}