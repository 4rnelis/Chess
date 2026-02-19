namespace Chess.Engine.Structures;

public enum PIECE_COLOR {BLACK = 0, WHITE = 1, NONE = 2};
public enum PIECE_TYPE {PAWN, ROOK, BISHOP, KNIGHT, KING, QUEEN, NONE};

/// <summary>
/// This struct represents a playing piece
/// </summary> 
public readonly struct Piece
{
    public readonly PIECE_COLOR PC;
    public readonly PIECE_TYPE PT;

    public static readonly Piece Empty = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);

    public Piece(PIECE_COLOR pc, PIECE_TYPE pt)
    {
        PC = pc;
        PT = pt;
    }

    public override string ToString()
    {
        return $"{PC}, {PT}";
    }
}

