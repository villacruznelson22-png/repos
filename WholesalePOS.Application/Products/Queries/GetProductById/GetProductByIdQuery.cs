using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalePOS.Application.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<ProductDto>
    {

        //This is the parameter to be be pass on the query
        public Guid Id { get; set; }
    }
}
