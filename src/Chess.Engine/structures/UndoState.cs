namespace Chess.Engine.Structures;

public struct UndoState(
    Piece source,
    Piece target,
    PIECE_COLOR sideToMove,
    Castling castlingRights,
    int enPassant,
    int prevHalfMoveClock,
    int prevFullMoveNumber,
    int prevKingBlack,
    int prevKingWhite,
    CastlingUndo castlingUndo = default)
{
    public readonly Piece Source = source;
    public readonly Piece Target = target;
    public readonly PIECE_COLOR SideToMove = sideToMove;
    public readonly Castling PrevCastlingRights = castlingRights;
    public readonly int PrevEnPassant = enPassant;
    public readonly int PrevHalfMoveClock = prevHalfMoveClock;
    public readonly int PrevFullMoveNumber = prevFullMoveNumber;
    public CastlingUndo CastlingUndo = castlingUndo;
    public readonly int PrevKingBlack = prevKingBlack;
    public readonly int PrevKingWhite = prevKingWhite;

    public override readonly string ToString()
    {
        return $"Source: {Source}, Target: {Target}, SideToMove: {SideToMove}, PrevCastlingRights: {PrevCastlingRights}, PrevEnPassant: {PrevEnPassant}, PrevHalfMoveClock: {PrevHalfMoveClock}, PrevFullMoveNumber: {PrevFullMoveNumber}, PrevKingPosition: [{PrevKingBlack}, {PrevKingWhite}]";
    }
}

public struct CastlingUndo
{
    public bool IsValid;
    public int KingFrom;
    public int KingTo;
    public int RookFrom;
    public int RookTo;
}
