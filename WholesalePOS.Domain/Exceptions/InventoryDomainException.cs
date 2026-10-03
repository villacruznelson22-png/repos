namespace WholesalePOS.Domain.Exceptions;

public class InventoryDomainException : DomainException
{
    public InventoryDomainException(string message)
        : base(message)
    {
    }
}
