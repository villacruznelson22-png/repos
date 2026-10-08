namespace WholesalePOS.Domain.Exceptions;

public sealed class UserDomainException : DomainException
{
    public UserDomainException(string message) : base(message) { }
}