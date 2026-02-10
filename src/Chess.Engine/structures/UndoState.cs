namespace Chess.Engine.Structures;

public struct UndoState(Piece source, Piece target, PIECE_COLOR sideToMove, Castling castlingRights, int enPassant, int[] prevKingPosition, int[,]? castlingPositions = null)
{
    public readonly Piece Source = source;
    public readonly Piece Target = target;
    public readonly PIECE_COLOR SideToMove = sideToMove;
    public readonly Castling PrevCastlingRights = castlingRights;
    public readonly int PrevEnPassant = enPassant;
    public int[,]? CastlingPositions = castlingPositions;
    public readonly int[] PrevKingPosition = prevKingPosition;
}