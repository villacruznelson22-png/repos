using WholesalePOS.Application.Users.Queries.GetUsers;

namespace WholesalePOS.Application.Tests.Users;

public sealed class GetUsersQueryValidatorTests
{
    [Fact]
    public void Validate_DefaultPagination_IsValid()
    {
        var result = new GetUsersQueryValidator().Validate(new GetUsersQuery());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_PageSizeAboveLimit_IsInvalid()
    {
        var result = new GetUsersQueryValidator().Validate(new GetUsersQuery { PageSize = 101 });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_NonPositivePageNumber_IsInvalid()
    {
        var result = new GetUsersQueryValidator().Validate(new GetUsersQuery { PageNumber = 0 });
        Assert.False(result.IsValid);
    }
}