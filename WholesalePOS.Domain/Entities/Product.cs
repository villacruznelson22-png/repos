using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WholesalePOS.Domain.Common;
using WholesalePOS.Domain.Exceptions;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities
{
    public class Product : BaseEntity
    {

        public string Name { get; private set; } = null!;

        public Barcode? Barcode { get; private set; }

        public Money DefaultSellingPrice { get; private set; } = null!;


        public Money SuggestedRetailPrice { get; private set; } = null!;

        public bool IsActive { get; private set; }

        public StockQuantity Stock { get; private set; } = null!;

        public byte[] Version { get; private set; } = null!;

        private Product()
        {
            // Used by EF Core
        }


        public Product(
        string name,
        Barcode? barcode,
        Money suggestedRetailPrice,
        Money defaultSellingPrice)
        {


            Id = Guid.NewGuid();


            ChangeName(name);

            ChangeBarcode(barcode);

            ChangeSuggestedRetailPrice(suggestedRetailPrice);
            ChangeDefaultSellingPrice(defaultSellingPrice);

            Activate();

            Stock = new StockQuantity(0);


        }
        public void ChangeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ProductDomainException(
                    "Product name cannot be empty.");

            Name = name.Trim();
        }

        public void ChangeDefaultSellingPrice(Money defaultSellingPrice)
        {

            if (defaultSellingPrice.Value < 0)
                throw new ProductDomainException(
                    "Default selling price cannot be negative.");

            DefaultSellingPrice = defaultSellingPrice;
        }

        public void ChangeSuggestedRetailPrice(Money suggestedRetailPrice)
        {
            if (suggestedRetailPrice.Value < 0)
                throw new ProductDomainException(
                    "Suggested retial price cannot be negative.");

            SuggestedRetailPrice = suggestedRetailPrice;
        }

        public void ChangeBarcode(Barcode? barcode)
        {
            Barcode = barcode;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void AddStock(decimal quantity)
        {
            Stock += quantity;
        }

        public void RemoveStock(decimal quantity)
        {
            Stock -= quantity;
        }
    }
}
