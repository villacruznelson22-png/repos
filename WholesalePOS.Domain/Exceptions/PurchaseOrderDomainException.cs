namespace WholesalePOS.Domain.Exceptions;

public class PurchaseOrderDomainException : DomainException
{
    public PurchaseOrderDomainException(string message)
        : base(message)
    {
    }
}