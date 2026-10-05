using System;
using WholesalePOS.Application.Common.Exceptions;

namespace WholesalePOS.Application.Common.Errors;

public static class SaleErrors
{
    public static NotFoundException NotFound(Guid id)
        => new($"Sale '{id}' was not found.");
}