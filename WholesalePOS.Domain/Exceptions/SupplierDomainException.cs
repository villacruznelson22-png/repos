namespace WholesalePOS.Domain.Exceptions;

public class SupplierDomainException : DomainException
{
    public SupplierDomainException(string message)
        : base(message)
    {
    }
}