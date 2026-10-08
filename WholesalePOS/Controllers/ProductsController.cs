using MediatR;
using Microsoft.AspNetCore.Mvc;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Products.Commands.CreateProduct;
using WholesalePOS.Application.Products.Commands.UpdateSellingPrice;
using WholesalePOS.Application.Products.Queries.GetProductById;
using WholesalePOS.Application.Products.Queries.GetProductByBarcode;
using WholesalePOS.Application.Products.Queries.GetProducts;

namespace WholesalePOS.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ISender _sender;

        public ProductsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            var id = await _sender.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                id);
        }


        [HttpPut("{id:guid}/selling-price")]
        public async Task<IActionResult> UpdateSellingPrice(
    Guid id,
    [FromBody] UpdateDefaultSellingPriceCommand command)
        {
            command.ProductId = id;

            await _sender.Send(command);

            return NoContent();
        }

     

        [HttpGet("barcode/{barcode}")]
        public async Task<ActionResult<ProductDto>> GetByBarcode(
            string barcode,
            CancellationToken cancellationToken)
        {
            var product = await _sender.Send(
                new GetProductByBarcodeQuery(barcode),
                cancellationToken);

            return Ok(product);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductDto>> GetById(Guid id)
        {
            var product = await _sender.Send(new GetProductByIdQuery
            {
                Id = id
            });

            return Ok(product);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ProductListItemDto>>> GetProducts([FromQuery] GetProductsQuery query)
        {
            var result = await _sender.Send(query);

            return Ok(result);
        }

    }
}