using System.Data.Common;
using System.Dynamic;
using Chess.Infrastructure;
using Chess.Logic;

public sealed class Board
{
    public Piece[] Layout { get; }

    public Board()
    {
        Layout = new Piece[64];
    }

    public Board(Piece[] layout)
    {
        Layout = layout;
    }

    /// <summary>
    /// Maps the moves to a specific piece based on their type
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns> <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public List<Move> GetValidMoves(int position)
    {
        Piece piece = Layout[position];
        Console.WriteLine($"{piece.PC}, {piece.PT}");
        return piece.PT switch
        {
            PIECE_TYPE.PAWN => GetValidMovesPawn(position, piece.PC),
            PIECE_TYPE.BISHOP => GetValidMovesBishop(position, piece.PC),
            PIECE_TYPE.ROOK => GetValidMovesRook(position, piece.PC),
            PIECE_TYPE.KNIGHT => GetValidMovesKnight(position, piece.PC),
            PIECE_TYPE.KING => GetValidMovesKing(position, piece.PC),
            PIECE_TYPE.QUEEN => GetValidMovesQueen(position, piece.PC),
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
    public List<Move> GetValidMovesPawn(int position, PIECE_COLOR color)
    {
        List<Move> moves = [];
        int to;
        switch (color) {
            case PIECE_COLOR.BLACK:
                {
                    // One step forward
                    to = position+8;
                    if ((uint)to < 64 && Layout[to].PT == PIECE_TYPE.NONE)
                    {
                        // Check for promotion
                        if ((to >> 3) == 7)
                        {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Promotion));
                        } else {
                            moves.Add(new Move(position, to, MOVE_FLAGS.None));
                            // Check if double push possible
                            to = position+16;
                            if ((uint)to < 64 && (position >> 3) == 1 && Layout[to].PT == PIECE_TYPE.NONE)
                            {
                                moves.Add(new Move(position, to, MOVE_FLAGS.DoublePawnPush));
                            }
                        }   
                    }
                    // Capture left
                    to = position+9;
                    if ((uint)to < 64 && (position & 7) != 7 && Layout[to].PC == PIECE_COLOR.WHITE)
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
                    if ((uint)to < 64 && (position & 7) != 0 && Layout[to].PC == PIECE_COLOR.WHITE)
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
                    if ((uint)to < 64 && Layout[to].PT == PIECE_TYPE.NONE)
                    {
                        // Check for promotion
                        if ((to >> 3) == 1)
                        {
                            moves.Add(new Move(position, to, MOVE_FLAGS.Promotion));
                        } else {
                            moves.Add(new Move(position, to, MOVE_FLAGS.None));
                            // Check if double push possible
                            to = position-16;
                            if ((uint)to < 64 && (position >> 3) == 6 && Layout[to].PT == PIECE_TYPE.NONE)
                            {
                                moves.Add(new Move(position, to, MOVE_FLAGS.DoublePawnPush));
                            }
                        }   
                    }
                    // Capture left
                    to = position-9;
                    if ((uint)to < 64 && (position & 7) != 7 && Layout[to].PC == PIECE_COLOR.BLACK)
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
                    if ((uint)to < 64 && (position & 7) != 0 && Layout[to].PC == PIECE_COLOR.BLACK)
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
    public List<Move> GetValidMovesBishop(int position, PIECE_COLOR color)
    {
        return SlidingMovesGeneralized(position, color, [+1, -1, +1, -1], [+1, +1, -1, -1]);
    }

    /// <summary>
    /// Rook move logic implementation
    /// </summary>
    /// <param name="position">The original position of a rook</param>
    /// <param name="color">The color of a rook</param>
    /// <returns>A list of all valid moves</returns>
    public List<Move> GetValidMovesRook(int position, PIECE_COLOR color)
    {
        return SlidingMovesGeneralized(position, color, [+1, -1, 0, 0], [0, 0, +1, -1]);
    }

    public List<Move> GetValidMovesKnight(int position, PIECE_COLOR color)
    {
        return StaticMovesGeneralized(position, color, [+1, -1, +1, -1, +2, +2, -2, -2], [+2, +2, -2, -2, +1, -1, +1, -1]);
    }

    /// <summary>
    /// King move logic implementation
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public List<Move> GetValidMovesKing(int position, PIECE_COLOR color)
    {
        return StaticMovesGeneralized(position, color, [-1, -1, -1, 0, 0, +1, +1, +1], [-1, 0, +1, -1, +1, -1, 0, +1]);
    }

    /// <summary>
    /// Queen move logic implementation. Alternatively: Rook + Bishop movement.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <returns></returns> <summary>
    public List<Move> GetValidMovesQueen(int position, PIECE_COLOR color)
    {
        return SlidingMovesGeneralized(position, color, [+1, -1, +1, -1, +1, -1, 0, 0], [+1, +1, -1, -1, 0, 0, +1, -1]);
    }


    /// <summary>
    /// Helper method to reuse in the sliding movement type pieces.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <param name="df">Direction of file: +1=right, -1=left, 0=stay</param>
    /// <param name="dr">Direction of rank: +1=down, -1=up, 0=stay</param>
    /// <returns></returns>
    public List<Move> SlidingMovesGeneralized(int position, PIECE_COLOR color, int[] df, int[] dr)
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
                target = Layout[tp];
                if (target.PC != PIECE_COLOR.NONE)
                {
                    if (target.PC == GetOppositeColor(color))
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
    public List<Move> StaticMovesGeneralized(int position, PIECE_COLOR color, int[] mf, int[] mr)
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
            tp = r*8+f;
            target = Layout[tp];

            if ((uint)f < 8 && (uint)r < 8) {
                if (target.PC != color)
                {
                    if (target.PC == GetOppositeColor(color)) {
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
    /// Helper for the logic where needed to find the opponent colour
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public PIECE_COLOR GetOppositeColor(PIECE_COLOR color)
    {
        if (color == PIECE_COLOR.WHITE)
        {
            return PIECE_COLOR.BLACK;
        } else {
            return PIECE_COLOR.WHITE;
        }
    }
}