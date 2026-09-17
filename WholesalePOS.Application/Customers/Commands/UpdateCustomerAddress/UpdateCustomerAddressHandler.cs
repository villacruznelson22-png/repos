using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Customers.Commands.UpdateCustomerAddress;

public class UpdateCustomerAddressHandler
    : IRequestHandler<UpdateCustomerAddressCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerAddressHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateCustomerAddressCommand request,
        CancellationToken cancellationToken)
    {
        var customer =
            await _customerRepository.GetByIdWithAddressesAsync(
                request.CustomerId,
                cancellationToken);

        if (customer is null)
            throw CustomerErrors.NotFound(request.CustomerId);

        var address = customer.Addresses
            .SingleOrDefault(x => x.Id == request.AddressId);

        if (address is null)
            throw new NotFoundException(
                $"Customer address '{request.AddressId}' was not found.");

        address.ChangeLabel(request.Address.Label);
        address.ChangeStreet(request.Address.Street);
        address.ChangeBarangay(request.Address.BarangayId);
        address.ChangePostalCode(request.Address.PostalCode);
        address.ChangeLandmark(request.Address.Landmark);
        address.ChangeLocation(
            request.Address.Latitude,
            request.Address.Longitude);

        if (request.Address.IsDefault)
        {
            customer.SetDefaultAddress(address.Id);
        }
        else
        {
            address.RemoveAsDefault();
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}