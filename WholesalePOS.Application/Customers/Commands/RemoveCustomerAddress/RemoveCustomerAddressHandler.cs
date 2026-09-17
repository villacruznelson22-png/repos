using MediatR;
using WholesalePOS.Application.Common.Exceptions;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Customers.Commands.RemoveCustomerAddress;

public class RemoveCustomerAddressHandler
    : IRequestHandler<RemoveCustomerAddressCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCustomerAddressHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        RemoveCustomerAddressCommand request,
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

        customer.RemoveAddress(request.AddressId);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}