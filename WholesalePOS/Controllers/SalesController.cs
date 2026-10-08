using MediatR;
using WholesalePOS.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Api.Contracts.Sales;
using WholesalePOS.Application.Sales.Commands.CancelSale;
using WholesalePOS.Application.Sales.Commands.CheckoutSale;
using WholesalePOS.Application.Sales.Commands.ConfirmSale;
using WholesalePOS.Application.Sales.Commands.CreateSale;
using WholesalePOS.Application.Sales.Commands.UpdateSale;
using WholesalePOS.Application.Sales.Queries.GetSaleById;
using WholesalePOS.Application.Sales.Queries.GetSales;

using ApplicationCheckoutPaymentRequest =
    WholesalePOS.Application.Sales.Commands.CheckoutSale.CheckoutPaymentRequest;

namespace WholesalePOS.Api.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISender _sender;

    public SalesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSaleCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);

        return Created($"/api/sales/{id}", id);
    }

    [HttpPut("{saleId:guid}")]
    public async Task<IActionResult> Update(
        Guid saleId,
        [FromBody] UpdateSaleCommand command,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { SaleId = saleId };

        await _sender.Send(updateCommand, cancellationToken);

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SaleListItemDto>>> Get(
        [FromQuery] GetSalesQuery query,
        CancellationToken cancellationToken)
    {
        var sales = await _sender.Send(
            query,
            cancellationToken);

        return Ok(sales);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var sale = await _sender.Send(
            new GetSaleByIdQuery(id),
            cancellationToken);

        return Ok(sale);
    }

    [HttpPost("{saleId:guid}/confirm")]
    public async Task<IActionResult> Confirm(
        Guid saleId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ConfirmSaleCommand(saleId),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{saleId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid saleId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new CancelSaleCommand(saleId),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{saleId:guid}/checkout")]
    public async Task<IActionResult> Checkout(
        Guid saleId,
        [FromBody] CheckoutSaleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CheckoutSaleCommand(
            saleId,
            request.IdempotencyKey,
            request.Payments
                .Select(payment => new ApplicationCheckoutPaymentRequest(
                    payment.Method,
                    payment.Amount,
                    payment.IdempotencyKey,
                    payment.ReferenceNumber))
                .ToList(),
            request.AllowNegativeInventory);

        await _sender.Send(command, cancellationToken);

        return NoContent();
    }
}
