using System.ComponentModel.DataAnnotations;

namespace UserAccess.Api.Models;

public sealed record CreateUserRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required, MinLength(2)] string DisplayName);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record UserResponse(
    Guid Id,
    string Email,
    string DisplayName);

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    UserResponse User);

public sealed record MessageResponse(string Message);
