using System.Diagnostics;

namespace Chess.Engine.Structures;

public readonly struct UndoState(Piece source, Piece target, Castling castlingRights, int? enPassant, int[] prevKingPosition)
{
    public readonly Piece Source = source;
    public readonly Piece Target = target;
    public readonly Castling PrevCastlingRights = castlingRights;
    public readonly int? PrevEnPassant = enPassant;
    public readonly int[] PrevKingPosition = prevKingPosition;
}