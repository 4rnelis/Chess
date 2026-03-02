namespace Chess.Server.Contracts.Auth;

public sealed record LoginRequest(string Name, string Password);
