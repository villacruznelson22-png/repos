namespace WholesalePOS.Application.Common.Exceptions;

public sealed class ForbiddenException : BusinessException
{
    public ForbiddenException(string message) : base(message) { }
}
