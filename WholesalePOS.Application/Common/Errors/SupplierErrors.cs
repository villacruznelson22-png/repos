using WholesalePOS.Application.Common.Exceptions;

namespace WholesalePOS.Application.Common.Errors;

public static class SupplierErrors
{
    public static NotFoundException NotFound(Guid id)
        => new($"Supplier '{id}' was not found.");
}