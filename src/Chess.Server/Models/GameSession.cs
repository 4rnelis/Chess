using Chess.Engine.Structures;

namespace Chess.Server.Models;

public sealed class GameSession
{
    public required string GameId { get; init; }
    public required Board Board { get; init; }
    public required string WhiteConnectionId { get; init; }
    public required string BlackConnectionId { get; init; }
    public List<string> MovesUci { get; } = new();
    public object SyncRoot { get; } = new();
}
