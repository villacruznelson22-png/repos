using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Suppliers.Commands.DeactivateSupplier;

public class DeactivateSupplierHandler
    : IRequestHandler<DeactivateSupplierCommand>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeactivateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (supplier is null)
        {
            throw SupplierErrors.NotFound(request.Id);
        }

        supplier.Deactivate();

        _supplierRepository.Update(supplier);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}