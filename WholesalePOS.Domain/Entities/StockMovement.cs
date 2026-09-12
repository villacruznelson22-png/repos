using WholesalePOS.Domain.Enums;
using WholesalePOS.Domain.ValueObjects;

namespace WholesalePOS.Domain.Entities;

public class StockMovement
{
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public StockMovementType Type { get; private set; }

    public StockMovementQuantity Quantity { get; private set; }
    public StockMovementDirection Direction { get; private set; }

    public DateTime OccurredAt { get; private set; }

    public bool IsIncrease => Type switch
    {
        StockMovementType.Purchase => true,
        StockMovementType.CustomerReturn => true,

        StockMovementType.Sale => false,
        StockMovementType.Damage => false,

        StockMovementType.Adjustment =>
            throw new InvalidOperationException(
                "Adjustment movement requires an explicit direction."),

        _ => throw new ArgumentOutOfRangeException()
    };

    public decimal SignedQuantity =>
    Direction == StockMovementDirection.Increase
        ? Quantity.Value
        : -Quantity.Value;



    public StockMovement(
        Guid productId,
        StockMovementType type,
        StockMovementDirection direction,
        StockMovementQuantity quantity)
    {

        ValidateDirection(type, direction);
        Id = Guid.NewGuid();
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        Direction = direction;
        OccurredAt = DateTime.UtcNow;

    }

 
    private static void ValidateDirection(
    StockMovementType type,
    StockMovementDirection direction)
    {
        switch (type)
        {
            case StockMovementType.Purchase:
            case StockMovementType.CustomerReturn:

                if (direction != StockMovementDirection.Increase)
                    throw new ArgumentException(
                        $"{type} must increase stock.");

                break;

            case StockMovementType.Sale:
            case StockMovementType.Damage:

                if (direction != StockMovementDirection.Decrease)
                    throw new ArgumentException(
                        $"{type} must decrease stock.");

                break;

            case StockMovementType.Adjustment:
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(type));
        }
    }
}