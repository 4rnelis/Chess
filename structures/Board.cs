using System.Data.Common;
using System.Dynamic;
using Chess.Infrastructure;
using Chess.Logic;

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
    public Piece[] Layout { get; }
    public Castling CastlingRights { get; private set; } = Castling.WhiteKingSide | Castling.WhiteQueenSide | Castling.BlackKingSide | Castling.BlackQueenSide;
    public int? EnPassantSq;

    public Board()
    {
        Layout = new Piece[64];
    }

    public Board(Piece[] layout)
    {
        Layout = layout;
    }

}