using MediatR;
using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Application.Sales.Commands.CreateSale;

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
}