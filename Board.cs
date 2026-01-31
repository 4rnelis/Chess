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
    /// <param name="color">The colour of a pawn</param>
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
    public List<Move> GetValidMovesBishop(int position, PIECE_COLOR color)
    {
        return [];
    }
    public List<Move> GetValidMovesRook(int position, PIECE_COLOR color)
    {
        return [];
    }
    public List<Move> GetValidMovesKnight(int position, PIECE_COLOR color)
    {
        return [];
    }
    public List<Move> GetValidMovesKing(int position, PIECE_COLOR color)
    {
        return [];
    }
    public List<Move> GetValidMovesQueen(int position, PIECE_COLOR color)
    {
        return [];
    }
}