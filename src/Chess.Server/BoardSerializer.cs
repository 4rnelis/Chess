using Chess.Engine.Structures;

namespace Chess.Server;

public static class BoardSerializer
{
    public static string[] ToClientBoard(Piece[] layout)
    {
        var result = new string[64];
        for (int i = 0; i < layout.Length; i++)
        {
            var piece = layout[i];
            if (piece.PC == PIECE_COLOR.NONE || piece.PT == PIECE_TYPE.NONE)
            {
                result[i] = "--";
                continue;
            }

            char color = piece.PC == PIECE_COLOR.WHITE ? 'w' : 'b';
            char type = piece.PT switch
            {
                PIECE_TYPE.PAWN => 'P',
                PIECE_TYPE.KNIGHT => 'N',
                PIECE_TYPE.BISHOP => 'B',
                PIECE_TYPE.ROOK => 'R',
                PIECE_TYPE.QUEEN => 'Q',
                PIECE_TYPE.KING => 'K',
                _ => '-'
            };
            result[i] = $"{color}{type}";
        }
        return result;
    }

    public static string CastlingToFen(Castling rights)
    {
        if (rights == Castling.None)
        {
            return "-";
        }

        Span<char> buffer = stackalloc char[4];
        int idx = 0;
        if (rights.HasFlag(Castling.WhiteKingSide)) buffer[idx++] = 'K';
        if (rights.HasFlag(Castling.WhiteQueenSide)) buffer[idx++] = 'Q';
        if (rights.HasFlag(Castling.BlackKingSide)) buffer[idx++] = 'k';
        if (rights.HasFlag(Castling.BlackQueenSide)) buffer[idx++] = 'q';
        return new string(buffer[..idx]);
    }

    public static string SideToMove(PIECE_COLOR color)
    {
        return color == PIECE_COLOR.WHITE ? "white" : "black";
    }
}
