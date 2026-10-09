using System.ComponentModel.DataAnnotations;

namespace WholesalePOS.Api.Contracts.Sales;

public sealed record VoidSaleRequest(
    [property: Required]
    [property: StringLength(500, MinimumLength = 1)]
    string Reason);
