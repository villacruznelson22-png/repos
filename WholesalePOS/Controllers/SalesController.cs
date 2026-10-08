using MediatR;
using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Api.Contracts.Sales;
using WholesalePOS.Application.Sales.Commands.CheckoutSale;
using WholesalePOS.Application.Sales.Commands.CreateSale;

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
        var id = await _sender.Send(
            command,
            cancellationToken);

        return Created(
            $"/api/sales/{id}",
            id);
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

        await _sender.Send(
            command,
            cancellationToken);

        return NoContent();
    }
}
