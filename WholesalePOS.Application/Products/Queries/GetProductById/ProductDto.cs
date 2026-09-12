using System;
using System.Collections.Generic;
using System.Text;

namespace WholesalePOS.Application.Products.Queries.GetProductById
{
    public class ProductDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Barcode { get; set; }

        public decimal SellingPrice { get; set; }
    }
}
