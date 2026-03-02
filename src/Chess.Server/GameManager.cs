using System.Collections.Concurrent;
using Chess.Engine;
using Chess.Engine.Logic;
using Chess.Engine.Structures;
using Chess.Server.Contracts;

namespace Chess.Server;

public sealed class GameManager
{
    private readonly ConcurrentQueue<string> _waitingPlayers = new();
    private readonly ConcurrentDictionary<string, byte> _waitingSet = new();
    private readonly ConcurrentDictionary<string, GameSession> _games = new();
    private readonly ConcurrentDictionary<string, string> _connectionToGame = new();
    private readonly object _matchLock = new();

    public (GameSession? game, string? youAre, string? opponentId) QueuePlayer(string connectionId)
    {
        lock (_matchLock)
        {
            while (_waitingPlayers.TryDequeue(out var opponentId))
            {
                if (!_waitingSet.TryRemove(opponentId, out _))
                {
                    continue;
                }
                if (string.Equals(opponentId, connectionId, StringComparison.Ordinal))
                {
                    continue;
                }

                var gameId = Guid.NewGuid().ToString("N");
                var board = new Board(Format.ImportFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"));

                var game = new GameSession
                {
                    GameId = gameId,
                    Board = board,
                    WhiteConnectionId = opponentId,
                    BlackConnectionId = connectionId
                };

                _games[gameId] = game;
                _connectionToGame[opponentId] = gameId;
                _connectionToGame[connectionId] = gameId;

                return (game, "black", opponentId);
            }

            _waitingSet[connectionId] = 0;
            _waitingPlayers.Enqueue(connectionId);
            return (null, null, null);
        }
    }

    public bool TryGetGame(string gameId, out GameSession game)
    {
        return _games.TryGetValue(gameId, out game!);
    }

    public bool TryGetGameForConnection(string connectionId, out GameSession game)
    {
        if (_connectionToGame.TryGetValue(connectionId, out var gameId))
        {
            return _games.TryGetValue(gameId, out game!);
        }
        game = null!;
        return false;
    }

    public bool TryMakeMove(
        string connectionId,
        string gameId,
        string uci,
        out string? error,
        out GameStateDto? state)
    {
        error = null;
        state = null;

        if (!_games.TryGetValue(gameId, out var game))
        {
            error = "Game not found.";
            return false;
        }

        lock (game.SyncRoot)
        {
            // Check if the connection is part of the game
            if (game.WhiteConnectionId != connectionId && game.BlackConnectionId != connectionId)
            {
                error = "You are not part of this game.";
                return false;
            }

            // Check the current moving side
            var expected = game.Board.SideToMove == PIECE_COLOR.WHITE
                ? game.WhiteConnectionId
                : game.BlackConnectionId;

            // Checks if the current moving side is correct
            if (!string.Equals(expected, connectionId, StringComparison.Ordinal))
            {
                error = "Not your turn.";
                return false;
            }

            // Validate UCI format
            if (string.IsNullOrWhiteSpace(uci) || (uci.Length != 4 && uci.Length != 5))
            {
                error = "UCI must be length 4 or 5.";
                return false;
            }


            var from = ParseSquare(uci[0], uci[1]);
            Console.WriteLine($"from square: {from}");
            var movingPieceColor = game.Board.Layout[from].PC;
            // Format.PrintBoard(game.Board.Layout);  
            var expectedColor = game.Board.SideToMove;
            if (movingPieceColor == PIECE_COLOR.NONE)
            {
                error = "No piece on that square.";
                return false;
            }
            Console.WriteLine($"moving piece color: {movingPieceColor}, expected color: {expectedColor}");
            if (movingPieceColor != expectedColor)
            {
                error = "You must move your own piece.";
                return false;
            }

            if (!game.Board.TryMakeUciMove(uci, out error))
            {
                return false;
            }

            game.MovesUci.Add(uci);

            var isMateBlack = MateChecker.CheckMate(game.Board, PIECE_COLOR.BLACK);
            var isMateWhite = MateChecker.CheckMate(game.Board, PIECE_COLOR.WHITE);
            var isMate = game.Board.SideToMove == PIECE_COLOR.WHITE ? isMateWhite : isMateBlack;
            Console.WriteLine($"is mate black: {isMateBlack}, is mate white: {isMateWhite}, is mate: {isMate}");
            state = BuildState(game, uci, isMate);
            return true;
        }
    }

    public GameStateDto BuildState(GameSession game, string? lastMoveUci, bool? isCheckmateOverride = null)
    {
        lock (game.SyncRoot)
        {
            var isMate = isCheckmateOverride ?? MateChecker.CheckMate(game.Board, game.Board.SideToMove);
            return new GameStateDto
            {
                GameId = game.GameId,
                SideToMove = BoardSerializer.SideToMove(game.Board.SideToMove),
                Board = BoardSerializer.ToClientBoard(game.Board.Layout),
                LastMoveUci = lastMoveUci,
                CastlingRights = BoardSerializer.CastlingToFen(game.Board.CastlingRights),
                EnPassantSquare = game.Board.EnPassantSq,
                IsCheckmate = isMate
            };
        }
    }

    public string? RemovePlayer(string connectionId, out string? gameId)
    {
        gameId = null;
        if (_waitingSet.TryRemove(connectionId, out _))
        {
            return null;
        }
        if (_connectionToGame.TryRemove(connectionId, out var existingGameId))
        {
            gameId = existingGameId;
            if (_games.TryRemove(existingGameId, out var game))
            {
                var opponentId = game.WhiteConnectionId == connectionId
                    ? game.BlackConnectionId
                    : game.WhiteConnectionId;
                _connectionToGame.TryRemove(opponentId, out _);
                return opponentId;
            }
        }

        return null;
    }

    private static int ParseSquare(char fileChar, char rankChar)
    {
        return ('8' - rankChar) * 8 + (fileChar - 'a');
    }
}
