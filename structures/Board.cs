using Chess.Logic;
namespace Chess.Structures;

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
    public int? EnPassantSq { get; internal set; }
    
    // Caching king positions for faster checking
    public int[] KingPosition {get; internal set; } = [4, 60];
    public UndoState UndoState {get; internal set; }

    // // Acess to modify the board to internal MoveMaker
    // public bool MakeMove(Move move) => MoveMaker.MakeMove(this, move);
    // public void UndoMove(Move move) => MoveMaker.UndoMove(this, move, UndoState, new int[2, 2]);

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