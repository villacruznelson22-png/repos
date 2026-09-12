using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalePOS.Application.Common.Models;
using WholesalePOS.Application.Products.Queries.GetProductById;

namespace WholesalePOS.Application.Products.Queries.GetProducts
{
    public class GetProductsQuery: PaginationRequest, IRequest<PagedResult<ProductListItemDto>>
    {
       
    }
}
