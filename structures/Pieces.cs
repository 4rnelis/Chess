namespace Chess.Infrastructure;

public enum PIECE_COLOR {BLACK, WHITE, NONE};
public enum PIECE_TYPE {PAWN, ROOK, BISHOP, KNIGHT, KING, QUEEN, NONE};

/// <summary>
/// This class represents a structure of a playing piece, and its move logic
/// </summary> 
public class Piece(PIECE_COLOR pc, PIECE_TYPE pt)
{
    public PIECE_COLOR PC { get; } = pc;
    public PIECE_TYPE PT { get; } = pt;
}

