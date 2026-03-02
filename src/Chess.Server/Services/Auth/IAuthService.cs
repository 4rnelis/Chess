using Chess.Server.Contracts.Auth;

namespace Chess.Server.Services.Auth;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<AuthUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
