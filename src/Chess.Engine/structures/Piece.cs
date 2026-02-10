namespace Chess.Engine.Structures;

public enum PIECE_COLOR {BLACK = 0, WHITE = 1, NONE = 2};
public enum PIECE_TYPE {PAWN, ROOK, BISHOP, KNIGHT, KING, QUEEN, NONE};

/// <summary>
/// This struct represents a playing piece
/// </summary> 
public struct Piece(PIECE_COLOR pc, PIECE_TYPE pt)
{
    public PIECE_COLOR PC { get; } = pc;
    public PIECE_TYPE PT { get; } = pt;

    public override string ToString()
    {
        return $"{PC}, {PT}";
    }
}

