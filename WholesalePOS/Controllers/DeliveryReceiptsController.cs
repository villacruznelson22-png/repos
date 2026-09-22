using MediatR;
using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Application.DeliveryReceipts.Commands.CancelDeliveryReceipt;
using WholesalePOS.Application.DeliveryReceipts.Commands.CreateDeliveryReceipt;
using WholesalePOS.Application.DeliveryReceipts.Commands.PostDeliveryReceipt;
using WholesalePOS.Application.DeliveryReceipts.Queries.GetDeliveryReceiptById;

namespace WholesalePOS.Api.Controllers;

[ApiController]
[Route("api/delivery-receipts")]
public class DeliveryReceiptsController : ControllerBase
{
    private readonly ISender _sender;

    public DeliveryReceiptsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDeliveryReceiptCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
            command,
            cancellationToken);

        return Created(
            $"/api/delivery-receipts/{id}",
            id);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeliveryReceiptDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deliveryReceipt = await _sender.Send(
            new GetDeliveryReceiptByIdQuery(id),
            cancellationToken);

        return Ok(deliveryReceipt);
    }

    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> Post(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new PostDeliveryReceiptCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new CancelDeliveryReceiptCommand(id),
            cancellationToken);

        return NoContent();
    }
}