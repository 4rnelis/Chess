using System.Drawing;
using Chess.Structures;

namespace Chess.Logic;

internal static class MoveMaker
{

    /// <summary>
    /// Takes a UCI string as an input, parses the coordinates to a 1D array, and checks if the requested move is valid.
    /// If yes, return the Move object for further logic.
    /// It needs to deal with promotions later on.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="UCI"></param>
    /// <returns></returns>
    public static Move? ParseAndValidateUCI(Board board, string UCI)
    {
        int from = ParseSquare(UCI[0], UCI[1]);
        int to = ParseSquare(UCI[2], UCI[3]);

        List<Move> moves = MovesGenerator.GetValidMoves(board, from);

        Move? move = moves.FirstOrDefault(
            m => m.Source == from && m.Target == to
        );

        return move;
    }

    public static int ParseSquare(char fc, char rc)
    {
        return ('8'-rc)*8+(fc-'a');
    }

    /// <summary>
    /// I wanted to put this outside of the board object, such as the pattern of Model and Logic would be separated, but due to security
    /// concerns and the goal of having a reliable source of truth, the moveMaking logic stays here.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="move"></param>
    /// <returns></returns>
    internal static bool MakeMove(Board board, Move? move)
    {
        if (move == null)
        {
            return false;
        }

        PIECE_COLOR color = board.Layout[move.Source].PC;
        PIECE_TYPE type = board.Layout[move.Source].PT;

        
        // 1. Check validity
        // 2. Store the state in undoState
        UndoState undoState = new(board.Layout[move.Source], board.Layout[move.Target], board.CastlingRights, board.EnPassantSq, board.KingPosition);
        int[,] undoCastling = new int[2,2];

        // 3. Make the move
        if (move.Flags == MOVE_FLAGS.None) 
        {
            board.Layout[move.Target] = board.Layout[move.Source];
            board.Layout[move.Source] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
        }
        if ((move.Flags & MOVE_FLAGS.Capture) != 0)
        {
            if ((move.Flags & MOVE_FLAGS.EnPassant) != 0) 
            {        
                if (color == PIECE_COLOR.BLACK)
                {
                    board.Layout[move.Target+8] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                }
                if (color == PIECE_COLOR.WHITE)
                {
                    board.Layout[move.Target-8] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                }
            }
            board.Layout[move.Target] = board.Layout[move.Source];
            board.Layout[move.Source] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
        }
        if ((move.Flags & MOVE_FLAGS.Castling) != 0)
        {
            // Remove the flags and switch depending which side/color
            if (color == PIECE_COLOR.BLACK)
            {
                board.CastlingRights &= ~Castling.BlackKingSide;
                board.CastlingRights &= ~Castling.BlackQueenSide;
                
                if (move.Source == 0 || move.Target == 0)
                {
                    board.Layout[2] = board.Layout[4];
                    board.Layout[4] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                    board.Layout[3] = board.Layout[0];
                    board.Layout[0] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                    board.KingPosition[0] = 2;
                    undoCastling = new int[2,2] {{2, 4}, {3, 0}};
                }
                if (move.Source == 7 || move.Target == 7)
                {
                    board.Layout[6] = board.Layout[4];
                    board.Layout[4] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                    board.Layout[5] = board.Layout[7];
                    board.Layout[7] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                    // Update cached king position
                    board.KingPosition[0] = 6;
                    undoCastling = new int[2,2] {{6, 4}, {5, 7}};
                }
            }

            if (color == PIECE_COLOR.WHITE)
            {
                board.CastlingRights &= ~Castling.WhiteKingSide;
                board.CastlingRights &= ~Castling.WhiteQueenSide;
                
                if (move.Source == 56 || move.Target == 56)
                {
                    board.Layout[58] = board.Layout[60];
                    board.Layout[60] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                    board.Layout[59] = board.Layout[56];
                    board.Layout[56] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                    board.KingPosition[1] = 58;
                    undoCastling = new int[2,2] {{58, 60}, {59, 56}};
                }
                if (move.Source == 63 || move.Target == 63)
                {
                    board.Layout[62] = board.Layout[60];
                    board.Layout[60] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                    board.Layout[61] = board.Layout[63];
                    board.Layout[63] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                    board.KingPosition[1] = 62;
                    undoCastling = new int[2,2] {{62, 60}, {61, 63}};
                }
            }
        }
        if ((move.Flags & MOVE_FLAGS.Promotion) != 0) {}

        if ((move.Flags & MOVE_FLAGS.DoublePawnPush) != 0)
        {
            board.Layout[move.Target] = board.Layout[move.Source];
            board.Layout[move.Source] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
            if (color == PIECE_COLOR.BLACK)
            {
                board.EnPassantSq = move.Source + 8;
            } 
            if (color == PIECE_COLOR.WHITE) 
            {
                board.EnPassantSq = move.Source - 8;
            }
        } else { board.EnPassantSq = -1; }

        if ( type == PIECE_TYPE.KING )
        {
            board.KingPosition[(int)color] = move.Target;
        }

        // 4. Check if king is threatened. Should add King square caching
        if (ThreatenedChecker.IsThreatened(board, board.KingPosition[(int)color], color))
        {
            UndoMove(board, move, undoState, undoCastling);
            return false;
        }

        // 5. If yes -> UndoMove, return false; else -> return true (or the String of the move)
        return true;
    }



    internal static void UndoMove(Board board, Move move, UndoState undoState, int[,] undoCastling)
    {
        // Restore the previous variables to a board.
        board.CastlingRights = undoState.PrevCastlingRights;
        board.EnPassantSq = undoState.PrevEnPassant;
        board.KingPosition = undoState.PrevKingPosition;

        // Undo castling from previously gathered stored variable
        if ((move.Flags & MOVE_FLAGS.Castling) != 0)
        {
            // Switch the positions back
            board.Layout[undoCastling[0,1]] = board.Layout[undoCastling[0,0]];
            board.Layout[undoCastling[1,1]] = board.Layout[undoCastling[1,0]];

        } else 
        {
            if ((move.Flags & MOVE_FLAGS.EnPassant) != 0)
            {
                if (undoState.Source.PC == PIECE_COLOR.BLACK)
                {
                    board.Layout[move.Target+8] = new Piece(PIECE_COLOR.WHITE, PIECE_TYPE.PAWN);
                }
                if (undoState.Source.PC == PIECE_COLOR.WHITE)
                {
                    board.Layout[move.Target-8] = new Piece(PIECE_COLOR.BLACK, PIECE_TYPE.PAWN);
                }
                board.Layout[move.Target] = undoState.Target;
                board.Layout[move.Source] = undoState.Source;
            }
        }
    }
}