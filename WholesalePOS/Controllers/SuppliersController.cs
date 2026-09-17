using MediatR;
using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Application.Suppliers.Commands.ActivateSupplier;
using WholesalePOS.Application.Suppliers.Commands.CreateSupplier;
using WholesalePOS.Application.Suppliers.Commands.DeactivateSupplier;
using WholesalePOS.Application.Suppliers.Commands.UpdateSupplier;
using WholesalePOS.Application.Suppliers.Queries.GetSupplierById;

namespace WholesalePOS.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISender _sender;

    public SuppliersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierCommand command,
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
    public async Task<ActionResult<SupplierDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var supplier = await _sender.Send(
            new GetSupplierByIdQuery(id),
            cancellationToken);

        return Ok(supplier);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(
                "Route ID does not match command ID.");
        }

        await _sender.Send(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ActivateSupplierCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeactivateSupplierCommand(id),
            cancellationToken);

        return NoContent();
    }
}