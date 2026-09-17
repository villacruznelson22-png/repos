using MediatR;
using WholesalePOS.Application.Common.Errors;
using WholesalePOS.Application.Interfaces;

namespace WholesalePOS.Application.Customers.Queries.GetCustomerById;

public class GetCustomerByIdHandler
    : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdHandler(
        ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer =
            await _customerRepository.GetByIdWithAddressesAsync(
                request.Id,
                cancellationToken);

        if (customer is null)
            throw CustomerErrors.NotFound(request.Id);

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            ContactNumber = customer.ContactNumber,
            Notes = customer.Notes,
            IsActive = customer.IsActive,

            Addresses = customer.Addresses
                .Select(address => new CustomerAddressResponseDto
                {
                    Id = address.Id,
                    Label = address.Label,
                    Street = address.Street,
                    BarangayId = address.BarangayId,
                    Barangay = address.Barangay.Name,
                    CityMunicipality =
                        address.Barangay.CityMunicipality.Name,
                    Province =
                        address.Barangay.CityMunicipality.Province?.Name,
                    Region =
                        address.Barangay.CityMunicipality.Region.Name,
                    PostalCode = address.PostalCode,
                    Landmark = address.Landmark,
                    Latitude = address.Latitude,
                    Longitude = address.Longitude,
                    IsDefault = address.IsDefault
                })
                .ToList()
        };
    }
}