namespace WholesalePOS.Application.Common.Exceptions;

public class ConcurrencyException : ConflictException
{
    public ConcurrencyException(string message) : base(message)
    {
    }
}