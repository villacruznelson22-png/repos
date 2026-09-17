using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Customers.Commands.AddCustomerAddress;

public class AddCustomerAddressHandler
    : IRequestHandler<AddCustomerAddressCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCustomerAddressHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        AddCustomerAddressCommand request,
        CancellationToken cancellationToken)
    {
        var customer =
            await _customerRepository.GetByIdWithAddressesAsync(
                request.CustomerId,
                cancellationToken);

        if (customer is null)
            throw CustomerErrors.NotFound(request.CustomerId);

        var address = new CustomerAddress(
            customer.Id,
            request.Address.Label,
            request.Address.Street,
            request.Address.BarangayId,
            request.Address.PostalCode,
            request.Address.Landmark,
            request.Address.Latitude,
            request.Address.Longitude,
            request.Address.IsDefault);

        customer.AddAddress(address);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return address.Id;
    }
}