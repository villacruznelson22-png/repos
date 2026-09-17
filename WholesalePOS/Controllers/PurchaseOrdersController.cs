using MediatR;
using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Application.PurchaseOrders.Commands.CancelPurchaseOrder;
using WholesalePOS.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using WholesalePOS.Application.PurchaseOrders.Commands.PostPurchaseOrder;
using WholesalePOS.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;
using WholesalePOS.Application.PurchaseOrders.Queries.GetPurchaseOrderById;

namespace WholesalePOS.Api.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public PurchaseOrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseOrderCommand command,
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
    public async Task<ActionResult<PurchaseOrderDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var purchaseOrder = await _sender.Send(
            new GetPurchaseOrderByIdQuery(id),
            cancellationToken);

        return Ok(purchaseOrder);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdatePurchaseOrderCommand command,
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

    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> Post(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new PostPurchaseOrderCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new CancelPurchaseOrderCommand(id),
            cancellationToken);

        return NoContent();
    }
}