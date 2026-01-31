namespace Chess.Infrastructure;

public enum PIECE_COLOR {BLACK, WHITE, NONE};
public enum PIECE_TYPE {PAWN, ROOK, BISHOP, KNIGHT, KING, QUEEN, NONE};

/// <summary>
/// This class represents a structure of a playing piece, and its move logic
/// </summary> 
public class Piece
{
    public PIECE_COLOR PC { get; }
    public PIECE_TYPE PT { get; }

    public Piece(PIECE_COLOR pc, PIECE_TYPE pt)
    {
        PC = pc;
        PT = pt;
    }

    public bool GetValidMoves(int[,] coords)
    {
        switch (this.PT)
        {
            case PIECE_TYPE.PAWN:
                return GetValidMovesPawn();
            case PIECE_TYPE.BISHOP:
                return GetValidMovesBishop();
            case PIECE_TYPE.ROOK:
                return GetValidMovesRook();
            case PIECE_TYPE.KNIGHT:
                return GetValidMovesKnight();
            case PIECE_TYPE.KING:
                return GetValidMovesKing();
            case PIECE_TYPE.QUEEN:
                return GetValidMovesQueen();
            default:
                return false;
        }
    }

    public bool GetValidMovesPawn()
    {
        return false;
    }
    public bool GetValidMovesBishop()
    {
        return false;
    }
    public bool GetValidMovesRook()
    {
        return false;
    }
    public bool GetValidMovesKnight()
    {
        return false;
    }
    public bool GetValidMovesKing()
    {
        return false;
    }
    public bool GetValidMovesQueen()
    {
        return false;
    }
}
