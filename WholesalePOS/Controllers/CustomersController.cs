using MediatR;
using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Application.Customers.Commands.AddCustomerAddress;
using WholesalePOS.Application.Customers.Commands.CreateCustomer;
using WholesalePOS.Application.Customers.Commands.RemoveCustomerAddress;
using WholesalePOS.Application.Customers.Commands.UpdateCustomerAddress;
using WholesalePOS.Application.Customers.DTOs;
using WholesalePOS.Application.Customers.Queries.GetCustomerById;

namespace WholesalePOS.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            id);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var customer = await _sender.Send(
            new GetCustomerByIdQuery
            {
                Id = id
            },
            cancellationToken);

        return Ok(customer);
    }

    [HttpPost("{id:guid}/addresses")]
    public async Task<IActionResult> AddAddress(
    Guid id,
    [FromBody] CustomerAddressDto address,
    CancellationToken cancellationToken)
    {
        var addressId = await _sender.Send(
            new AddCustomerAddressCommand(
                id,
                address),
            cancellationToken);

        return Ok(addressId);
    }

    [HttpPut("{id:guid}/addresses/{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(
    Guid id,
    Guid addressId,
    [FromBody] CustomerAddressDto address,
    CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateCustomerAddressCommand(
                id,
                addressId,
                address),
            cancellationToken);

        return NoContent();
    }


    [HttpDelete("{id:guid}/addresses/{addressId:guid}")]
    public async Task<IActionResult> RemoveAddress(
    Guid id,
    Guid addressId,
    CancellationToken cancellationToken)
    {
        await _sender.Send(
            new RemoveCustomerAddressCommand(
                id,
                addressId),
            cancellationToken);

        return NoContent();
    }
}