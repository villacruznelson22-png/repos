namespace WholesalePOS.Domain.Exceptions;

public class CustomerAddressDomainException : DomainException
{
    public CustomerAddressDomainException(string message)
        : base(message)
    {
    }
}