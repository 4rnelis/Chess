using Chess.Infrastructure;

public static class MovesGenerator
{
    /// <summary>
    /// Maps the moves to a specific piece based on their type
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns> <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static List<Move> GetValidMoves(Board board, int position)
    {
        Piece piece = board.Layout[position];
        Console.WriteLine($"{piece.PC}, {piece.PT}");
        return piece.PT switch
        {
            PIECE_TYPE.PAWN => GetValidMovesPawn(board, position, piece.PC),
            PIECE_TYPE.BISHOP => GetValidMovesBishop(board, position, piece.PC),
            PIECE_TYPE.ROOK => GetValidMovesRook(board, position, piece.PC),
            PIECE_TYPE.KNIGHT => GetValidMovesKnight(board, position, piece.PC),
            PIECE_TYPE.KING => GetValidMovesKing(board, position, piece.PC),
            PIECE_TYPE.QUEEN => GetValidMovesQueen(board, position, piece.PC),
            _ => [],
        };
    }

    /// <summary>
    /// Pawn move logic implementation.
    /// TODO: EnPassant and Castling
    /// </summary>
    /// <param name="position">The original position of a pawn</param>
    /// <param name="color">The color of a pawn</param>
    /// <returns>A list of all valid moves</returns> 
    public static List<Move> GetValidMovesPawn(Board board, int position, PIECE_COLOR color)
    {
        List<Move> moves = [];
        int to;
        switch (color) {
            case PIECE_COLOR.BLACK:
                {
                    // One step forward
                    to = position+8;
                    if ((uint)to < 64 && board.Layout[to].PT == PIECE_TYPE.NONE)
                    {
                        // Check for promotion
                        if ((to >> 3) == 7)
                        {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Promotion));
                        } else {
                            moves.Add(new Move(position, to, MOVE_FLAGS.None));
                            // Check if double push possible
                            to = position+16;
                            if ((uint)to < 64 && (position >> 3) == 1 && board.Layout[to].PT == PIECE_TYPE.NONE)
                            {
                                moves.Add(new Move(position, to, MOVE_FLAGS.DoublePawnPush));
                            }
                        }   
                    }
                    // Capture left
                    to = position+9;
                    if (((uint)to < 64 && (position & 7) != 7 && board.Layout[to].PC == PIECE_COLOR.WHITE) || to == board.EnPassantSq)
                    {
                        if ((to >> 3) == 7)
                        {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Promotion | MOVE_FLAGS.Capture));
                        } else {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Capture));
                        }
                    }
                    // Capture right
                    to = position+7;
                    if (((uint)to < 64 && (position & 7) != 0 && board.Layout[to].PC == PIECE_COLOR.WHITE) || to == board.EnPassantSq)
                    {
                        if ((to >> 3) == 7)
                        {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Promotion | MOVE_FLAGS.Capture));
                        } else {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Capture));
                        }
                    }
                    return moves;
                }
            case PIECE_COLOR.WHITE:
                {
                    // One step forward
                    to = position-8;
                    if ((uint)to < 64 && board.Layout[to].PT == PIECE_TYPE.NONE)
                    {
                        // Check for promotion
                        if ((to >> 3) == 1)
                        {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Promotion));
                        } else {
                            moves.Add(new Move(position, to, MOVE_FLAGS.None));
                            // Check if double push possible
                            to = position-16;
                            if ((uint)to < 64 && (position >> 3) == 6 && board.Layout[to].PT == PIECE_TYPE.NONE)
                            {
                                moves.Add(new Move(position, to, MOVE_FLAGS.DoublePawnPush));
                            }
                        }   
                    }
                    // Capture left
                    to = position-9;
                    if (((uint)to < 64 && (position & 7) != 7 && board.Layout[to].PC == PIECE_COLOR.BLACK) || to == board.EnPassantSq)
                    {
                        if ((to >> 3) == 7)
                        {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Promotion | MOVE_FLAGS.Capture));
                        } else {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Capture));
                        }
                    }
                    // Capture right
                    to = position-7;
                    if (((uint)to < 64 && (position & 7) != 0 && board.Layout[to].PC == PIECE_COLOR.BLACK) || to == board.EnPassantSq)
                    {
                        if ((to >> 3) == 7)
                        {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Promotion | MOVE_FLAGS.Capture));
                        } else {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Capture));
                        }
                    }
                    return moves;
                }
            default:
                return [];
        };
    }

    /// <summary>
    /// Bishop move logic implementation
    /// </summary>
    /// <param name="position">The original position of a bishop</param>
    /// <param name="color">The color of a bishop</param>
    /// <returns>A list of all valid moves</returns>
    public static List<Move> GetValidMovesBishop(Board board, int position, PIECE_COLOR color)
    {
        return SlidingMovesGeneralized(board, position, color, [+1, -1, +1, -1], [+1, +1, -1, -1]);
    }

    /// <summary>
    /// Rook move logic implementation
    /// </summary>
    /// <param name="position">The original position of a rook</param>
    /// <param name="color">The color of a rook</param>
    /// <returns>A list of all valid moves</returns>
    public static List<Move> GetValidMovesRook(Board board, int position, PIECE_COLOR color)
    {
        List<Move> moves = SlidingMovesGeneralized(board, position, color, [+1, -1, 0, 0], [0, 0, +1, -1]);
        moves.AddRange(CastlingMoves(board, position, color));
        return moves;
    }

    public static List<Move> GetValidMovesKnight(Board board, int position, PIECE_COLOR color)
    {
        return StaticMovesGeneralized(board, position, color, [+1, -1, +1, -1, +2, +2, -2, -2], [+2, +2, -2, -2, +1, -1, +1, -1]);
    }

    /// <summary>
    /// King move logic implementation
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static List<Move> GetValidMovesKing(Board board, int position, PIECE_COLOR color)
    {
        List<Move> moves = StaticMovesGeneralized(board, position, color, [-1, -1, -1, 0, 0, +1, +1, +1], [-1, 0, +1, -1, +1, -1, 0, +1]);
        moves.AddRange(CastlingMoves(board, position, color));
        return moves;
    }

    /// <summary>
    /// Queen move logic implementation. Alternatively: Rook + Bishop movement.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <returns></returns> <summary>
    public static List<Move> GetValidMovesQueen(Board board, int position, PIECE_COLOR color)
    {
        return SlidingMovesGeneralized(board, position, color, [+1, -1, +1, -1, +1, -1, 0, 0], [+1, +1, -1, -1, 0, 0, +1, -1]);
    }


    /// <summary>
    /// Helper method to reuse in the sliding movement type pieces.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <param name="df">Direction of file: +1=right, -1=left, 0=stay</param>
    /// <param name="dr">Direction of rank: +1=down, -1=up, 0=stay</param>
    /// <returns></returns>
    public static List<Move> SlidingMovesGeneralized(Board board, int position, PIECE_COLOR color, int[] df, int[] dr)
    {
        int length = df.Length;

        List<Move> moves = [];
        // Source file
        int sf = position & 7;
        // Source rank
        int sr = position >> 3;
        int f, r, tp;
        Piece target;

        // Iterate over each direction
        for (int dir = 0; dir < length; dir++)
        {
            // file/rank = source f/r + a move in a direction
            f = sf + df[dir];
            r = sr + dr[dir];

            while ((uint)f < 8 && (uint)r < 8)
            {
                tp = r*8+f;
                target = board.Layout[tp];
                if (target.PC != PIECE_COLOR.NONE)
                {
                    if (target.PC == Board.GetOppositeColor(color))
                    {
                        moves.Add(new Move(position, tp, MOVE_FLAGS.Capture));
                    }
                    break;
                } else {
                    moves.Add(new Move(position, tp, MOVE_FLAGS.None));
                }

                f += df[dir];
                r += dr[dir];
            } 
        }
        return moves;
    }

    /// <summary>
    /// Helper method to reuse in the static movement type pieces.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <param name="mf">moves in file</param>
    /// <param name="mr">moves in rank</param>
    /// <returns></returns>
    public static List<Move> StaticMovesGeneralized(Board board, int position, PIECE_COLOR color, int[] mf, int[] mr)
    {
        int length = mf.Length;

        List<Move> moves = [];
        // Source file
        int sf = position & 7;
        // Source rank
        int sr = position >> 3;
        int f, r, tp;
        Piece target;

        for (int mv = 0; mv < length; mv++)
        {
            f = sf + mf[mv];
            r = sr + mr[mv];

            if ((uint)f < 8 && (uint)r < 8) {
                tp = r*8+f;
                target = board.Layout[tp];
                if (target.PC != color)
                {
                    if (target.PC == Board.GetOppositeColor(color)) {
                        moves.Add(new Move(position, tp, MOVE_FLAGS.Capture));
                    } else {
                        moves.Add(new Move(position, tp, MOVE_FLAGS.None));
                    }
                } 
            }
        }
        return moves;
    }

    /// <summary>
    /// Checks if castling move is valid for Rook and King
    /// </summary>
    /// <param name="board"></param>
    /// <param name="position"></param>
    /// <param name="target"></param>
    /// <param name="color"></param>
    /// <returns></returns> 
    public static List<Move> CastlingMoves(Board board, int position, PIECE_COLOR color)
    {
        List<Move> moves = [];
        PIECE_TYPE pt = board.Layout[position].PT;

        switch (color)
        {
            case PIECE_COLOR.BLACK:
                // Check the flag
                if ((board.CastlingRights & Castling.BlackQueenSide) != 0)
                {
                    // Checks if the path is empty and not under direct threat
                    if ((board.Layout[1].PT == PIECE_TYPE.NONE) && (board.Layout[2].PT == PIECE_TYPE.NONE) && (board.Layout[3].PT == PIECE_TYPE.NONE) &&
                        !(ThreatenedChecker.IsThreatened(board, 4, PIECE_COLOR.BLACK) && ThreatenedChecker.IsThreatened(board, 1, PIECE_COLOR.BLACK) && ThreatenedChecker.IsThreatened(board, 2, PIECE_COLOR.BLACK) && ThreatenedChecker.IsThreatened(board, 3, PIECE_COLOR.BLACK)))
                    {
                        // Assigns the move either to the king or to the rook
                        if (pt == PIECE_TYPE.KING)
                        {
                            moves.Add(new Move(4, 2, MOVE_FLAGS.Castling));
                        }
                        if (pt == PIECE_TYPE.ROOK)
                        {
                            moves.Add(new Move(0, 3, MOVE_FLAGS.Castling));
                        }
                    }        
                }
                if ((board.CastlingRights & Castling.BlackKingSide) != 0)
                {
                    if ((board.Layout[5].PT == PIECE_TYPE.NONE) && (board.Layout[6].PT == PIECE_TYPE.NONE) &&
                        !(ThreatenedChecker.IsThreatened(board, 4, PIECE_COLOR.BLACK) && ThreatenedChecker.IsThreatened(board, 5, PIECE_COLOR.BLACK) && ThreatenedChecker.IsThreatened(board, 6, PIECE_COLOR.BLACK)))
                    {
                        if (pt == PIECE_TYPE.KING)
                        {
                            moves.Add(new Move(4, 6, MOVE_FLAGS.Castling));
                        }
                        if (pt == PIECE_TYPE.ROOK)
                        {
                            moves.Add(new Move(7, 5, MOVE_FLAGS.Castling));
                        }
                    }
                }
                return moves;
            case PIECE_COLOR.WHITE:
                if ((board.CastlingRights & Castling.WhiteQueenSide) != 0)
                {
                    if ((board.Layout[57].PT == PIECE_TYPE.NONE) && (board.Layout[58].PT == PIECE_TYPE.NONE) && (board.Layout[59].PT == PIECE_TYPE.NONE) &&
                        !(ThreatenedChecker.IsThreatened(board, 60, PIECE_COLOR.WHITE) && ThreatenedChecker.IsThreatened(board, 57, PIECE_COLOR.WHITE) && ThreatenedChecker.IsThreatened(board, 58, PIECE_COLOR.WHITE) && ThreatenedChecker.IsThreatened(board, 59, PIECE_COLOR.WHITE)))
                    {
                        if (pt == PIECE_TYPE.KING)
                        {
                            moves.Add(new Move(60, 58, MOVE_FLAGS.Castling));
                        }
                        if (pt == PIECE_TYPE.ROOK)
                        {
                            moves.Add(new Move(56, 59, MOVE_FLAGS.Castling));
                        }
                    }
                }
                if ((board.CastlingRights & Castling.WhiteKingSide) != 0)
                {
                    if ((board.Layout[61].PT == PIECE_TYPE.NONE) && (board.Layout[62].PT == PIECE_TYPE.NONE) &&
                        !(ThreatenedChecker.IsThreatened(board, 60, PIECE_COLOR.WHITE) && ThreatenedChecker.IsThreatened(board, 61, PIECE_COLOR.WHITE) && ThreatenedChecker.IsThreatened(board, 62, PIECE_COLOR.WHITE)))
                    {
                        if (pt == PIECE_TYPE.KING)
                        {
                            moves.Add(new Move(60, 62, MOVE_FLAGS.Castling));
                        }
                        if (pt == PIECE_TYPE.ROOK)
                        {
                            moves.Add(new Move(63, 61, MOVE_FLAGS.Castling));
                        }
                    }
                }
                return moves;
        }
        return [];
    }
}