namespace WholesalePOS.Domain.Exceptions;

public class DeliveryReceiptDomainException : DomainException
{
    public DeliveryReceiptDomainException(string message)
        : base(message)
    {
    }
}