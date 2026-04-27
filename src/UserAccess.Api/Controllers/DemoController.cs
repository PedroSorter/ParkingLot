using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAccess.Api.Models;

namespace UserAccess.Api.Controllers;

[ApiController]
[Route("api/demo")]
public sealed class DemoController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public ActionResult<MessageResponse> Public()
        => Ok(new MessageResponse("This endpoint is public and does not require a token."));

    [HttpGet("protected")]
    [Authorize]
    public ActionResult<MessageResponse> Protected()
    {
        var displayName = User.FindFirstValue(ClaimTypes.Name) ?? "authenticated user";
        return Ok(new MessageResponse($"Hello {displayName}, this endpoint requires a valid bearer token."));
    }
}
