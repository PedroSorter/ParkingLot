namespace UserAccess.Api.Services;

public sealed record UserAccount(
    Guid Id,
    string Email,
    string DisplayName,
    string PasswordHash);
