using System;
using System.Collections.Generic;
using System.Text;
using WholesalePOS.Application.Common.Exceptions;

namespace WholesalePOS.Application.Common.Errors
{
    public static class ProductErrors
    {
        public static NotFoundException NotFound(Guid id)
        => new($"Product '{id}' was not found.");

        public static ConflictException DuplicateBarcode(string barcode)
            => new($"Barcode '{barcode}' already exists.");
    }
}
