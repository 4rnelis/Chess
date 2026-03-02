namespace Chess.Server.Contracts.Auth;

public sealed record AuthUserDto(Guid Id, string Name, int Rating);
