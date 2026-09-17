using System;
using WholesalePOS.Application.Common.Exceptions;

namespace WholesalePOS.Application.Common.Errors;

public static class CustomerErrors
{
    public static NotFoundException NotFound(Guid id)
        => new($"Customer '{id}' was not found.");
}