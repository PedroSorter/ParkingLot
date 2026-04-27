using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAccess.Api.Models;
using UserAccess.Api.Services;

namespace UserAccess.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(IUserStore users, JwtTokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public ActionResult<UserResponse> Register(CreateUserRequest request)
    {
        if (!users.TryCreate(request.Email, request.Password, request.DisplayName, out var user))
        {
            return Conflict(new MessageResponse("A user with this email already exists."));
        }

        return CreatedAtAction(nameof(Me), new { }, ToResponse(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        if (!users.TryValidateCredentials(request.Email, request.Password, out var user))
        {
            return Unauthorized(new MessageResponse("Invalid email or password."));
        }

        var (accessToken, expiresAt) = tokens.CreateToken(user);
        return Ok(new LoginResponse(accessToken, expiresAt, ToResponse(user)));
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserResponse> Me()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.NameId)
            ?? User.FindFirstValue("nameid");

        if (!Guid.TryParse(userIdValue, out var userId) || !users.TryGetById(userId, out var user))
        {
            return Unauthorized(new MessageResponse("Authenticated user was not found."));
        }

        return Ok(ToResponse(user));
    }

    private static UserResponse ToResponse(UserAccount user)
        => new(user.Id, user.Email, user.DisplayName);
}
