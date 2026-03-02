namespace Chess.Server.Contracts;


/// <summary>
/// Represents the data transfer object for a started game.
/// </summary> 
public sealed class GameStartedDto
{
    public required string GameId { get; init; }
    public required string YouAre { get; init; }
    public required string OpponentId { get; init; }
}
