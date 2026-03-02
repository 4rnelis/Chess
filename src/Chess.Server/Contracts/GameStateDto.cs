namespace Chess.Server.Contracts;

public sealed class GameStateDto
{
    public required string GameId { get; init; }
    public required string SideToMove { get; init; }
    public required string[] Board { get; init; }
    public string? LastMoveUci { get; init; }
    public required string CastlingRights { get; init; }
    public required int EnPassantSquare { get; init; }
    public required bool IsCheckmate { get; init; }
}
