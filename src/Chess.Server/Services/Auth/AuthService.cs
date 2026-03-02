using Chess.Server.Contracts.Auth;

namespace Chess.Server.Services.Auth;

// Template placeholder for auth business logic.
public sealed class AuthService : IAuthService
{
    public Task<AuthResultDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AuthResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AuthUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
