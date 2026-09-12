namespace WholesalePOS.Application.Common.Models;

public abstract class PaginationRequest
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}