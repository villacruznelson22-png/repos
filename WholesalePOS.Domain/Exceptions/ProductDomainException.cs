namespace WholesalePOS.Domain.Exceptions;

public class ProductDomainException : DomainException
{
    public ProductDomainException(string message)
        : base(message)
    {
    }
}