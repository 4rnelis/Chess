using Chess.Engine.Logic;

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
    public UndoState UndoState {get; internal set; }

    // // Acess to modify the board to internal MoveMaker
    public UndoState? MakeMove(Move move) => MoveMaker.MakeMove(this, move);
    public void UndoMove(Move move, UndoState undoState) => MoveMaker.UndoMove(this, move, undoState);
    public void UndoMove(Move move) => MoveMaker.UndoMove(this, move, UndoState);


    public Board()
    {
        Layout = new Piece[64];
    }

    public Board(Piece[] layout)
    {
        Layout = layout;
    }

    /// <summary>
    /// Helper for the logic where needed to find the opponent colour
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static PIECE_COLOR GetOppositeColor(PIECE_COLOR color)
    {
        if (color == PIECE_COLOR.WHITE)
        {
            return PIECE_COLOR.BLACK;
        } else {
            return PIECE_COLOR.WHITE;
        }
    }
}