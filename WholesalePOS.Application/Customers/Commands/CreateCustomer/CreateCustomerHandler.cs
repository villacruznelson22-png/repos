using MediatR;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Application.Customers.Commands.CreateCustomer;

public class CreateCustomerHandler
    : IRequestHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = new Customer(
            request.Name,
            request.ContactNumber,
            request.Notes);

        if (request.Address is not null)
        {
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
        }

        await _customerRepository.AddAsync(
            customer,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return customer.Id;
    }
}