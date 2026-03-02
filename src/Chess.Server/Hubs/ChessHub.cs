using Chess.Server.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Chess.Server.Hubs;

public sealed class ChessHub : Hub
{
    private readonly GameManager _games;

    public ChessHub(GameManager games)
    {
        _games = games;
    }

    public async Task JoinQueue()
    {
        var connectionId = Context.ConnectionId;
        if (_games.TryGetGameForConnection(connectionId, out var existingGame))
        {
            var state = _games.BuildState(existingGame, lastMoveUci: null);
            await Clients.Caller.SendAsync("MoveApplied", state);
            return;
        }

        var (game, youAre, opponentId) = _games.QueuePlayer(connectionId);
        if (game is null)
        {
            await Clients.Caller.SendAsync("Queued");
            return;
        }

        var opponentColor = youAre == "white" ? "black" : "white";
        var callerStarted = new GameStartedDto
        {
            GameId = game.GameId,
            YouAre = youAre!,
            OpponentId = opponentId!
        };
        var opponentStarted = new GameStartedDto
        {
            GameId = game.GameId,
            YouAre = opponentColor,
            OpponentId = connectionId
        };

        await Groups.AddToGroupAsync(connectionId, game.GameId);
        await Groups.AddToGroupAsync(opponentId!, game.GameId);

        await Clients.Caller.SendAsync("GameStarted", callerStarted);
        await Clients.Client(opponentId!).SendAsync("GameStarted", opponentStarted);

        var gameState = _games.BuildState(game, lastMoveUci: null, isCheckmateOverride: false);
        await Clients.Group(game.GameId).SendAsync("MoveApplied", gameState);
    }

    public async Task MakeMove(string gameId, string uci)
    {
        if (_games.TryMakeMove(Context.ConnectionId, gameId, uci, out var error, out var state))
        {
            await Clients.Group(gameId).SendAsync("MoveApplied", state);
            return;
        }

        await Clients.Caller.SendAsync("InvalidMove", error ?? "Unknown error.");
    }

    public async Task GetState(string gameId)
    {
        if (_games.TryGetGame(gameId, out var game))
        {
            var state = _games.BuildState(game, lastMoveUci: null);
            await Clients.Caller.SendAsync("MoveApplied", state);
            return;
        }

        await Clients.Caller.SendAsync("InvalidMove", "Game not found.");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var opponentId = _games.RemovePlayer(Context.ConnectionId, out var gameId);
        if (opponentId is not null && gameId is not null)
        {
            await Clients.Client(opponentId).SendAsync("OpponentDisconnected", gameId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
