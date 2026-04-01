using Chess.Server.Contracts.Auth;
using Chess.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Chess.Server.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public Task<AuthResultDto> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        return _authService.RegisterAsync(request, cancellationToken);
    }

    [HttpPost("login")]
    public Task<AuthResultDto> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        return _authService.LoginAsync(request, cancellationToken);
    }

    [HttpPost("logout")]
    public Task Logout(CancellationToken cancellationToken)
    {
        return _authService.LogoutAsync(cancellationToken);
    }

    [HttpGet("me")]
    public Task<AuthUserDto?> Me(CancellationToken cancellationToken)
    {
        return _authService.GetCurrentUserAsync(cancellationToken);
    }
}
