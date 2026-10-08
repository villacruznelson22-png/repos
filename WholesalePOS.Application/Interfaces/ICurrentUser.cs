namespace WholesalePOS.Application.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
}