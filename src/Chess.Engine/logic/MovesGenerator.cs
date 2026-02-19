using Chess.Engine.Structures;

namespace Chess.Engine.Logic;

public static class MovesGenerator
{
    private const int MaxPieceMoves = 64;

    // Representing the rank and file direction changes for each piece
    private static readonly int[] BishopDf = [+1, -1, +1, -1];
    private static readonly int[] BishopDr = [+1, +1, -1, -1];
    private static readonly int[] RookDf = [+1, -1, 0, 0];
    private static readonly int[] RookDr = [0, 0, +1, -1];
    private static readonly int[] QueenDf = [+1, -1, +1, -1, +1, -1, 0, 0];
    private static readonly int[] QueenDr = [+1, +1, -1, -1, 0, 0, +1, -1];
    private static readonly int[] KnightMf = [+1, -1, +1, -1, +2, +2, -2, -2];
    private static readonly int[] KnightMr = [+2, +2, -2, -2, +1, -1, +1, -1];
    private static readonly int[] KingMf = [-1, -1, -1, 0, 0, +1, +1, +1];
    private static readonly int[] KingMr = [-1, 0, +1, -1, +1, -1, 0, +1];

    /// <summary>
    /// Get valid moves for a single piece
    /// </summary>
    /// <param name="board"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static List<Move> GetValidMoves(Board board, int position)
    {
        Span<Move> buffer = stackalloc Move[MaxPieceMoves];
        int count = GetValidMovesCount(board, position, buffer);
        return ToList(buffer, count);
    }

    /// <summary>
    /// Get the count of possible valid moves
    /// </summary>
    /// <param name="board"></param>
    /// <param name="position"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public static int GetValidMovesCount(Board board, int position, Span<Move> destination)
    {
        Piece piece = board.Layout[position];
        if (piece.PC == PIECE_COLOR.NONE)
            return 0;

        int count = 0;
        switch (piece.PT)
        {
            case PIECE_TYPE.PAWN:
                AddPawnMoves(board, position, piece.PC, destination, ref count);
                break;
            case PIECE_TYPE.BISHOP:
                AddSlidingMoves(board, position, piece.PC, BishopDf, BishopDr, destination, ref count);
                break;
            case PIECE_TYPE.ROOK:
                AddSlidingMoves(board, position, piece.PC, RookDf, RookDr, destination, ref count);
                break;
            case PIECE_TYPE.KNIGHT:
                AddKnightMoves(board, position, piece.PC, destination, ref count);
                break;
            case PIECE_TYPE.KING:
                AddKingMoves(board, position, piece.PC, destination, ref count);
                break;
            case PIECE_TYPE.QUEEN:
                AddSlidingMoves(board, position, piece.PC, QueenDf, QueenDr, destination, ref count);
                break;
        }

        return count;
    }

    public static List<Move> GenerateAllMoves(Board board)
    {
        var moves = new List<Move>(64);
        Span<Move> buffer = stackalloc Move[MaxPieceMoves];
        PIECE_COLOR side = board.SideToMove;

        for (int sq = 0; sq < 64; sq++)
        {
            if (board.Layout[sq].PC != side)
                continue;

            int count = GetValidMovesCount(board, sq, buffer);
            for (int i = 0; i < count; i++)
            {
                moves.Add(buffer[i]);
            }
        }

        return moves;
    }

    private static void AddPawnMoves(Board board, int position, PIECE_COLOR color, Span<Move> destination, ref int count)
    {
        int to;
        MOVE_FLAGS flags;

        if (color == PIECE_COLOR.BLACK)
        {
            to = position + 8;
            if ((uint)to < 64 && board.Layout[to].PT == PIECE_TYPE.NONE)
            {
                if ((to >> 3) == 7)
                {
                    AddPromotionMoves(position, to, MOVE_FLAGS.Promotion, destination, ref count);
                }
                else
                {
                    AddMove(destination, ref count, new Move(position, to, MOVE_FLAGS.None));
                    to = position + 16;
                    if ((uint)to < 64 && (position >> 3) == 1 && board.Layout[to].PT == PIECE_TYPE.NONE)
                    {
                        AddMove(destination, ref count, new Move(position, to, MOVE_FLAGS.DoublePawnPush));
                    }
                }
            }

            to = position + 9;
            if ((uint)to < 64 && (position & 7) != 7 && (board.Layout[to].PC == PIECE_COLOR.WHITE || to == board.EnPassantSq))
            {
                flags = MOVE_FLAGS.Capture;
                if (to == board.EnPassantSq)
                    flags |= MOVE_FLAGS.EnPassant;

                if ((to >> 3) == 7)
                {
                    AddPromotionMoves(position, to, flags | MOVE_FLAGS.Promotion, destination, ref count);
                }
                else
                {
                    AddMove(destination, ref count, new Move(position, to, flags));
                }
            }

            to = position + 7;
            if ((uint)to < 64 && (position & 7) != 0 && (board.Layout[to].PC == PIECE_COLOR.WHITE || to == board.EnPassantSq))
            {
                flags = MOVE_FLAGS.Capture;
                if (to == board.EnPassantSq)
                    flags |= MOVE_FLAGS.EnPassant;

                if ((to >> 3) == 7)
                {
                    AddPromotionMoves(position, to, flags | MOVE_FLAGS.Promotion, destination, ref count);
                }
                else
                {
                    AddMove(destination, ref count, new Move(position, to, flags));
                }
            }

            return;
        }

        if (color == PIECE_COLOR.WHITE)
        {
            to = position - 8;
            if ((uint)to < 64 && board.Layout[to].PT == PIECE_TYPE.NONE)
            {
                if ((to >> 3) == 0)
                {
                    AddPromotionMoves(position, to, MOVE_FLAGS.Promotion, destination, ref count);
                }
                else
                {
                    AddMove(destination, ref count, new Move(position, to, MOVE_FLAGS.None));
                    to = position - 16;
                    if ((uint)to < 64 && (position >> 3) == 6 && board.Layout[to].PT == PIECE_TYPE.NONE)
                    {
                        AddMove(destination, ref count, new Move(position, to, MOVE_FLAGS.DoublePawnPush));
                    }
                }
            }

            to = position - 9;
            if ((uint)to < 64 && (position & 7) != 0 && (board.Layout[to].PC == PIECE_COLOR.BLACK || to == board.EnPassantSq))
            {
                flags = MOVE_FLAGS.Capture;
                if (to == board.EnPassantSq)
                    flags |= MOVE_FLAGS.EnPassant;

                if ((to >> 3) == 0)
                {
                    AddPromotionMoves(position, to, flags | MOVE_FLAGS.Promotion, destination, ref count);
                }
                else
                {
                    AddMove(destination, ref count, new Move(position, to, flags));
                }
            }

            to = position - 7;
            if ((uint)to < 64 && (position & 7) != 7 && (board.Layout[to].PC == PIECE_COLOR.BLACK || to == board.EnPassantSq))
            {
                flags = MOVE_FLAGS.Capture;
                if (to == board.EnPassantSq)
                    flags |= MOVE_FLAGS.EnPassant;

                if ((to >> 3) == 0)
                {
                    AddPromotionMoves(position, to, flags | MOVE_FLAGS.Promotion, destination, ref count);
                }
                else
                {
                    AddMove(destination, ref count, new Move(position, to, flags));
                }
            }
        }
    }

    private static void AddKnightMoves(Board board, int position, PIECE_COLOR color, Span<Move> destination, ref int count)
    {
        int sf = position & 7;
        int sr = position >> 3;
        PIECE_COLOR opposite = Board.GetOppositeColor(color);

        for (int mv = 0; mv < 8; mv++)
        {
            int f = sf + KnightMf[mv];
            int r = sr + KnightMr[mv];
            if ((uint)f >= 8 || (uint)r >= 8)
                continue;

            int targetSquare = r * 8 + f;
            Piece target = board.Layout[targetSquare];
            if (target.PC == color)
                continue;

            AddMove(destination, ref count, target.PC == opposite
                ? new Move(position, targetSquare, MOVE_FLAGS.Capture)
                : new Move(position, targetSquare, MOVE_FLAGS.None));
        }
    }

    private static void AddKingMoves(Board board, int position, PIECE_COLOR color, Span<Move> destination, ref int count)
    {
        int sf = position & 7;
        int sr = position >> 3;
        PIECE_COLOR opposite = Board.GetOppositeColor(color);

        for (int mv = 0; mv < 8; mv++)
        {
            int f = sf + KingMf[mv];
            int r = sr + KingMr[mv];
            if ((uint)f >= 8 || (uint)r >= 8)
                continue;

            int targetSquare = r * 8 + f;
            Piece target = board.Layout[targetSquare];
            if (target.PC == color)
                continue;

            AddMove(destination, ref count, target.PC == opposite
                ? new Move(position, targetSquare, MOVE_FLAGS.Capture)
                : new Move(position, targetSquare, MOVE_FLAGS.None));
        }

        AddCastlingMoves(board, position, color, destination, ref count);
    }

    private static void AddSlidingMoves(Board board, int position, PIECE_COLOR color, int[] df, int[] dr, Span<Move> destination, ref int count)
    {
        PIECE_COLOR opposite = Board.GetOppositeColor(color);
        int sf = position & 7;
        int sr = position >> 3;

        for (int dir = 0; dir < df.Length; dir++) 
        {
            int f = sf + df[dir];
            int r = sr + dr[dir];

            while ((uint)f < 8 && (uint)r < 8)
            {
                int targetSquare = r * 8 + f;
                Piece target = board.Layout[targetSquare];
                if (target.PC != PIECE_COLOR.NONE)
                {
                    if (target.PC == opposite)
                    {
                        AddMove(destination, ref count, new Move(position, targetSquare, MOVE_FLAGS.Capture));
                    }
                    break;
                }

                AddMove(destination, ref count, new Move(position, targetSquare, MOVE_FLAGS.None));
                f += df[dir];
                r += dr[dir];
            }
        }
    }

    private static void AddCastlingMoves(Board board, int position, PIECE_COLOR color, Span<Move> destination, ref int count)
    {
        PIECE_TYPE pt = board.Layout[position].PT;
        if (pt != PIECE_TYPE.KING)
            return;

        if (color == PIECE_COLOR.BLACK)
        {
            bool startSafe = !ThreatenedChecker.IsThreatened(board, 4, PIECE_COLOR.BLACK);
            if ((board.CastlingRights & Castling.BlackQueenSide) != 0 &&
                board.Layout[1].PT == PIECE_TYPE.NONE &&
                board.Layout[2].PT == PIECE_TYPE.NONE &&
                board.Layout[3].PT == PIECE_TYPE.NONE &&
                startSafe &&
                !ThreatenedChecker.IsThreatened(board, 2, PIECE_COLOR.BLACK) &&
                !ThreatenedChecker.IsThreatened(board, 3, PIECE_COLOR.BLACK))
            {
                AddMove(destination, ref count, new Move(4, 0, MOVE_FLAGS.Castling));
            }

            if ((board.CastlingRights & Castling.BlackKingSide) != 0 &&
                board.Layout[5].PT == PIECE_TYPE.NONE &&
                board.Layout[6].PT == PIECE_TYPE.NONE &&
                startSafe &&
                !ThreatenedChecker.IsThreatened(board, 5, PIECE_COLOR.BLACK) &&
                !ThreatenedChecker.IsThreatened(board, 6, PIECE_COLOR.BLACK))
            {
                AddMove(destination, ref count, new Move(4, 7, MOVE_FLAGS.Castling));
            }
            return;
        }

        if (color == PIECE_COLOR.WHITE)
        {
            bool startSafe = !ThreatenedChecker.IsThreatened(board, 60, PIECE_COLOR.WHITE);
            if ((board.CastlingRights & Castling.WhiteQueenSide) != 0 &&
                board.Layout[57].PT == PIECE_TYPE.NONE &&
                board.Layout[58].PT == PIECE_TYPE.NONE &&
                board.Layout[59].PT == PIECE_TYPE.NONE &&
                startSafe &&
                !ThreatenedChecker.IsThreatened(board, 58, PIECE_COLOR.WHITE) &&
                !ThreatenedChecker.IsThreatened(board, 59, PIECE_COLOR.WHITE))
            {
                AddMove(destination, ref count, new Move(60, 56, MOVE_FLAGS.Castling));
            }

            if ((board.CastlingRights & Castling.WhiteKingSide) != 0 &&
                board.Layout[61].PT == PIECE_TYPE.NONE &&
                board.Layout[62].PT == PIECE_TYPE.NONE &&
                startSafe &&
                !ThreatenedChecker.IsThreatened(board, 61, PIECE_COLOR.WHITE) &&
                !ThreatenedChecker.IsThreatened(board, 62, PIECE_COLOR.WHITE))
            {
                AddMove(destination, ref count, new Move(60, 63, MOVE_FLAGS.Castling));
            }
        }
    }

    private static void AddPromotionMoves(int source, int target, MOVE_FLAGS flags, Span<Move> destination, ref int count)
    {
        AddMove(destination, ref count, new Move(source, target, flags, PIECE_TYPE.QUEEN));
        AddMove(destination, ref count, new Move(source, target, flags, PIECE_TYPE.ROOK));
        AddMove(destination, ref count, new Move(source, target, flags, PIECE_TYPE.BISHOP));
        AddMove(destination, ref count, new Move(source, target, flags, PIECE_TYPE.KNIGHT));
    }

    private static void AddMove(Span<Move> destination, ref int count, Move move)
    {
        if ((uint)count >= (uint)destination.Length)
            throw new InvalidOperationException("Move buffer is too small for generated moves.");

        destination[count++] = move;
    }

    private static List<Move> ToList(Span<Move> source, int count)
    {
        var moves = new List<Move>(count);
        for (int i = 0; i < count; i++)
        {
            moves.Add(source[i]);
        }
        return moves;
    }
}
