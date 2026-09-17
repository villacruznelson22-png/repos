namespace WholesalePOS.Domain.Exceptions;

public class CustomerDomainException : DomainException
{
    public CustomerDomainException(string message)
        : base(message)
    {
    }
}