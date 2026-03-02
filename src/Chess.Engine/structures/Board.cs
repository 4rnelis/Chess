using Chess.Engine.Logic;
using System.Runtime.CompilerServices;

namespace Chess.Engine.Structures;

[Flags]
public enum Castling
{
    None = 0,
    WhiteKingSide  = 1 << 0,
    WhiteQueenSide = 1 << 1,
    BlackKingSide  = 1 << 2,
    BlackQueenSide = 1 << 3
}

public sealed class Board
{
    public Piece[] Layout { get; internal set; }
    public Castling CastlingRights { get; internal set; } = Castling.WhiteKingSide | Castling.WhiteQueenSide | Castling.BlackKingSide | Castling.BlackQueenSide;
    public int EnPassantSq { get; internal set; } = -1;
    public PIECE_COLOR SideToMove { get; internal set; } = PIECE_COLOR.WHITE;
    
    // Caching king positions for faster checking
    public int[] KingPosition {get; internal set; } = [4, 60];
    public int HalfMoveClock { get; internal set; } = 0;
    public int FullMoveNumber { get; internal set; } = 1;
    public UndoState UndoState {get; internal set; }

    // // Acess to modify the board to internal MoveMaker
    public UndoState? MakeMove(Move move) => MoveMaker.MakeMove(this, move);
    public void UndoMove(Move move, UndoState undoState) => MoveMaker.UndoMove(this, move, undoState);
    public void UndoMove(Move move) => MoveMaker.UndoMove(this, move, UndoState);

    public bool TryMakeUciMove(string uci, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(uci) || (uci.Length != 4 && uci.Length != 5))
        {
            error = "UCI must be length 4 or 5.";
            return false;
        }

        var move = MoveMaker.ParseAndValidateUCI(this, uci);
        if (!move.HasValue)
        {
            error = "Illegal move.";
            return false;
        }

        var undo = MakeMove(move.Value);
        if (!undo.HasValue)
        {
            error = "Move leaves king in check.";
            return false;
        }

        return true;
    }


    public Board()
    {
        Layout = new Piece[64];
    }

    public Board(Piece[] layout)
    {
        Layout = layout;
        RefreshKingPositions();
    }

        public static Board FromFen(string fen)
    {
        return FromFenState(Format.ParseFEN(fen));
    }

    public static Board FromFenState(FenState state)
    {
        return new Board(state.Layout)
        {
            SideToMove = state.SideToMove,
            CastlingRights = state.CastlingRights,
            EnPassantSq = state.EnPassantSquare,
            HalfMoveClock = state.HalfMoveClock,
            FullMoveNumber = state.FullMoveNumber
        };
    }

    /// <summary>
    /// Helper for the logic where needed to find the opponent colour
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PIECE_COLOR GetOppositeColor(PIECE_COLOR color)
    {
        return color == PIECE_COLOR.WHITE ? PIECE_COLOR.BLACK : PIECE_COLOR.WHITE;
    }

    private void RefreshKingPositions()
    {
        for (int i = 0; i < Layout.Length; i++)
        {
            if (Layout[i].PT != PIECE_TYPE.KING)
            {
                continue;
            }

            if (Layout[i].PC == PIECE_COLOR.BLACK)
            {
                KingPosition[0] = i;
            }
            else if (Layout[i].PC == PIECE_COLOR.WHITE)
            {
                KingPosition[1] = i;
            }
        }
    }
}
