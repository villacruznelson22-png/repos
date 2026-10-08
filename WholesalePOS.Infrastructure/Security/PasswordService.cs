using Microsoft.AspNetCore.Identity;
using WholesalePOS.Application.Interfaces;
using WholesalePOS.Domain.Entities;

namespace WholesalePOS.Infrastructure.Security;

public sealed class PasswordService : IPasswordService
{
    private const string DummyPassword = "WholesalePOS-Authentication-Dummy-Password";

    private readonly PasswordHasher<User> _hasher = new();
    private readonly string _dummyHash;

    public PasswordService()
    {
        _dummyHash = _hasher.HashPassword(null!, DummyPassword);
    }

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return _hasher.HashPassword(null!, password);
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var hashToVerify = string.IsNullOrWhiteSpace(hash)
            ? _dummyHash
            : hash;

        var result = _hasher.VerifyHashedPassword(
            null!,
            hashToVerify,
            password);

        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}