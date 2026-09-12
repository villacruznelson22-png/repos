using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoelsalePOS.Domain.Common;

namespace WhoelsalePOS.Domain.Entities
{
    public class Product : BaseEntity
    {

        public string Name { get; private set; } = string.Empty;

        public string Barcode { get; private set; } = string.Empty;

        public decimal SellingPrice { get; private set; }

        public decimal CostPrice { get; private set; }

        public bool IsActive { get; private set; }

        public Product(
        string name,
        string barcode,
        decimal sellingPrice,
        decimal costPrice)
        {

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required.");

            if (sellingPrice < 0)
                throw new ArgumentException("Selling price cannot be negative.");

            if (costPrice < 0)
                throw new ArgumentException("Cost price cannot be negative.");


            Id = Guid.NewGuid();

            Name = name;

            Barcode = barcode;

            SellingPrice = sellingPrice;

            CostPrice = costPrice;

            IsActive = true;
        }
    }
}
