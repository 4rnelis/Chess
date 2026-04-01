using Chess.Server.Contracts.Auth;
using Chess.Server.Data;
using Chess.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Chess.Server.Services.Auth;

public sealed class AuthService
{
    private readonly AppDbContext _appDbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(AppDbContext appDbContext, IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor)
    {
        _appDbContext = appDbContext;
        _passwordHasher = passwordHasher;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        //Check if username is taken
        var userExists = await _appDbContext.Users
        .FirstOrDefaultAsync(u => u.Name == request.Name, cancellationToken);

        if (userExists is not null)
        {
            return await Task.FromResult(new AuthResultDto(
                false,
                "Name taken!",
                null
            ));
        }
    
        //Hash the password
        var hashed = _passwordHasher.HashPassword(new User(), request.Password);
        //Store the user in the database
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            PasswordHash = hashed,
            CreatedAtUtc = DateTime.UtcNow
        };
        _appDbContext.Users.Add(user);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        //Check if request cancelled before signing in
        if (cancellationToken.IsCancellationRequested)
        {
            return await Task.FromCanceled<AuthResultDto>(cancellationToken);
        }

        return await Task.FromResult(new AuthResultDto(
            true,
            null,
            new AuthUserDto(user.Id, user.Name, user.Rating, user.CreatedAtUtc)
        ));

    }

    public async Task<AuthResultDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var userExists = await _appDbContext.Users
        .FirstOrDefaultAsync(u => u.Name == request.Name, cancellationToken);

        //User exists
        if (userExists is null)
        {
            return await Task.FromResult(new AuthResultDto(
                false,
                "Wrong username or password",
                null
            ));
        }
        //Verify using Identity
        var verificationResult = _passwordHasher.VerifyHashedPassword(userExists, userExists.PasswordHash, request.Password);
        //Wrong password
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return await Task.FromResult(new AuthResultDto(
                false,
                "Wrong username or password",
                null
            ));
        }
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return await Task.FromResult(new AuthResultDto(
                false,
                "No active HTTP context",
                null
            ));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userExists.Id.ToString()),
            new(ClaimTypes.Name, userExists.Name)
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(600)
            });

        return await Task.FromResult(new AuthResultDto(
            true,
            null,
            new AuthUserDto(userExists.Id, userExists.Name, userExists.Rating, userExists.CreatedAtUtc)
        ));
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<AuthUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated is not true)
        {
            return null;
        }

        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        var user = await _appDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new AuthUserDto(user.Id, user.Name, user.Rating, user.CreatedAtUtc);
    }
}
