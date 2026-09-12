using System;
using System.Collections.Generic;
using System.Text;
using WholesalePOS.Domain.Entities;
using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Tests
{
    public class StockMovementTests
    {

        [Fact]
        public void Purchase_ShouldIncreaseStock()
        {
            var movement = new StockMovement(
                Guid.NewGuid(),
                StockMovementType.Purchase,
                StockMovementDirection.Increase,
                new StockMovementQuantity(10));

            Assert.Equal(
                10,
                movement.SignedQuantity);
        }

        [Fact]
        public void Sale_ShouldDecreaseStock()
        {
            var movement = new StockMovement(
                Guid.NewGuid(),
                StockMovementType.Sale,
                StockMovementDirection.Decrease,
                new StockMovementQuantity(2.5m));

            Assert.Equal(
                -2.5m,
                movement.SignedQuantity);
        }

        [Fact]
        public void Sale_ShouldNotAllowIncreaseDirection()
        {
            var action = () =>
                new StockMovement(
                    Guid.NewGuid(),
                    StockMovementType.Sale,
                    StockMovementDirection.Increase,
                    new StockMovementQuantity(10));

            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void Adjustment_ShouldAllowIncrease()
        {
            var movement = new StockMovement(
                Guid.NewGuid(),
                StockMovementType.Adjustment,
                StockMovementDirection.Increase,
                new StockMovementQuantity(5));

            Assert.Equal(
                5,
                movement.SignedQuantity);
        }

        //Add the corresponding decrease test afterward.

    }
}
