using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeHub.Api.Auth;

namespace ResumeHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        => Ok(await authService.RegisterAsync(request));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        => Ok(await authService.LoginAsync(request));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
        => Ok(await authService.RefreshAsync(request));
}
