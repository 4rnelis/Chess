namespace Chess.Server.Contracts.Auth;

public sealed record AuthResultDto(bool Success, string? Error, AuthUserDto? User);
