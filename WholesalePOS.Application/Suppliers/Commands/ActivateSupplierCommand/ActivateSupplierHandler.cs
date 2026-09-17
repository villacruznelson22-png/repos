using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Suppliers.Commands.ActivateSupplier;

public class ActivateSupplierHandler
    : IRequestHandler<ActivateSupplierCommand>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateSupplierHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ActivateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (supplier is null)
        {
            throw SupplierErrors.NotFound(request.Id);
        }

        supplier.Activate();

        _supplierRepository.Update(supplier);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}