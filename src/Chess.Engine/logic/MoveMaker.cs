using Chess.Engine.Structures;

namespace Chess.Engine.Logic;

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

        var candidates = moves
            .Where(m => m.Source == from && m.Target == to)
            .ToList();

        // Console.WriteLine($"Current move: from {from} to {to}");
        foreach (var move in moves )
        {
            // Console.WriteLine(move);
        }
        
        if(UCI.Length == 5)
        {
            return candidates.FirstOrDefault(m => m.Promotion == ParsePromotion(UCI[4]));
        }

        return candidates.Count == 1 ? candidates[0] : null;;
    }

    public static int ParseSquare(char fc, char rc)
    {
        return ('8'-rc)*8+(fc-'a');
    }

    public static PIECE_TYPE ParsePromotion(char u)
    {
        if (u == 'Q' || u == 'q')
        {
            return PIECE_TYPE.QUEEN;
        }
        if (u == 'R' || u == 'r')
        {
            return PIECE_TYPE.ROOK;
        }
        if (u == 'B' || u == 'b')
        {
            return PIECE_TYPE.BISHOP;
        }
        if (u == 'N' || u == 'n')
        {
            return PIECE_TYPE.KNIGHT;
        }
        return PIECE_TYPE.NONE;
    }

    /// <summary>
    /// I wanted to put this outside of the board object, such as the pattern of Model and Logic would be separated, but due to security
    /// concerns and the goal of having a reliable source of truth, the moveMaking logic stays here.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="move"></param>
    /// <returns></returns>
    internal static UndoState? MakeMove(Board board, Move move)
    {
        // Console.WriteLine($"MOVE: {move}, Source: {board.Layout[move.Source]}");
        // Console.WriteLine($"[Move Maker] En Passant Sq: {board.EnPassantSq}");
        PIECE_COLOR color = board.Layout[move.Source].PC;
        PIECE_TYPE type = board.Layout[move.Source].PT;
        // if (color == PIECE_COLOR.BLACK && type ==PIECE_TYPE.PAWN) {
        Format.PrintBoard(board.Layout);
        // }



        // 1. Check validity
        // 2. Store the state in undoState
        UndoState undoState = new(board.Layout[move.Source], board.Layout[move.Target], board.SideToMove, board.CastlingRights, board.EnPassantSq, board.KingPosition)
        {
            // 3. Make the move
            CastlingPositions = UpdateMove(board, move, color, type)
        };


        // Console.WriteLine($"{(int)color}, king position WHITE: {board.KingPosition[1]}, BLACK: {board.KingPosition[0]}");
        // 4. Check if king is threatened. Should add King square caching
        if (ThreatenedChecker.IsThreatened(board, board.KingPosition[(int)color], color))
        {
            // Console.WriteLine($"King is threatened!");
            UndoMove(board, move, undoState);
            return null;
        }

        board.UndoState = undoState;

        board.SideToMove = Board.GetOppositeColor(board.SideToMove);
        // 5. If yes -> UndoMove, return false; else -> return true (or the String of the move)
        // Console.WriteLine($"[Move Maker] En Passant Sq after: {board.EnPassantSq}");
        // Console.WriteLine("BOARD after the move");
        Format.PrintBoard(board.Layout);
        return undoState;
    }

    internal static void UndoMove(Board board, Move move, UndoState undoState)
    {

        // Restore the previous variables to a board.
        board.CastlingRights = undoState.PrevCastlingRights;
        board.EnPassantSq = undoState.PrevEnPassant;
        board.KingPosition = undoState.PrevKingPosition;
        int[,]? undoCastling = undoState.CastlingPositions;

        // Undo castling from previously gathered stored variable
        if ((move.Flags & MOVE_FLAGS.Castling) != 0)
        {
            if (undoCastling == null)
            {
                throw new Exception("undoCastling array is null in UndoMove. Something is wrong with castling logic!");
            }
            // Switch the positions back
            board.Layout[undoCastling[0,1]] = board.Layout[undoCastling[0,0]];
            board.Layout[undoCastling[0,0]] = new Piece(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
            board.Layout[undoCastling[1,1]] = board.Layout[undoCastling[1,0]];
            board.Layout[undoCastling[1,0]] = new Piece(PIECE_COLOR.NONE, PIECE_TYPE.NONE);

        } else 
        // Switch back the previous positions (En passant: Add a new piece on +8/-8 depending on color)
        {
            if ((move.Flags & MOVE_FLAGS.EnPassant) != 0)
            {
                if (undoState.Source.PC == PIECE_COLOR.BLACK)
                {
                    board.Layout[move.Target-8] = new Piece(PIECE_COLOR.WHITE, PIECE_TYPE.PAWN);
                }
                if (undoState.Source.PC == PIECE_COLOR.WHITE)
                {
                    board.Layout[move.Target+8] = new Piece(PIECE_COLOR.BLACK, PIECE_TYPE.PAWN);
                }
            }
            board.Layout[move.Target] = undoState.Target;
            board.Layout[move.Source] = undoState.Source;
        }

        board.SideToMove = undoState.SideToMove;
    }

    internal static int[,]? UpdateMove(Board board, Move move, PIECE_COLOR color, PIECE_TYPE type)
    {
        int[,]? undoCastling = null;
        if (move.Flags == MOVE_FLAGS.None) 
        {
            UpdateBaseMove(board, move);
        }
        if ((move.Flags & MOVE_FLAGS.Capture) != 0)
        {
            UpdateCapture(board, move, color);
        }
        if ((move.Flags & MOVE_FLAGS.Castling) != 0)
        {
            undoCastling = UpdateCastling(board, move, color);
        }
        if ((move.Flags & MOVE_FLAGS.Promotion) != 0)
        {
            UpdatePromotion(board, move, color);
        }
        if ((move.Flags & MOVE_FLAGS.DoublePawnPush) != 0)
        {
            UpdateDoublePawnPush(board, move, color);
        } else { board.EnPassantSq = -1; }
        if ( type == PIECE_TYPE.KING )
        {
            board.KingPosition[(int)color] = move.Target;
        }
        return undoCastling;
    }

    internal static void UpdateBaseMove(Board board, Move move)
    {

        board.Layout[move.Target] = board.Layout[move.Source];
        board.Layout[move.Source] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
        
    }

    internal static void UpdateCapture(Board board, Move move, PIECE_COLOR color)
    {

        if ((move.Flags & MOVE_FLAGS.EnPassant) != 0) 
        {        
            if (color == PIECE_COLOR.BLACK)
            {
                board.Layout[move.Target-8] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
            }
            if (color == PIECE_COLOR.WHITE)
            {
                board.Layout[move.Target+8] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
            }
        }           
        UpdateBaseMove(board, move);    
    }

    internal static int[,]? UpdateCastling(Board board, Move move, PIECE_COLOR color)
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
                return new int[2,2] {{2, 4}, {3, 0}};
            }
            if (move.Source == 7 || move.Target == 7)
            {
                board.Layout[6] = board.Layout[4];
                board.Layout[4] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                board.Layout[5] = board.Layout[7];
                board.Layout[7] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                // Update cached king position
                board.KingPosition[0] = 6;
                return new int[2,2] {{6, 4}, {5, 7}};
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
                return new int[2,2] {{58, 60}, {59, 56}};
            }
            if (move.Source == 63 || move.Target == 63)
            {
                board.Layout[62] = board.Layout[60];
                board.Layout[60] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                board.Layout[61] = board.Layout[63];
                board.Layout[63] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
                board.KingPosition[1] = 62;
                return new int[2,2] {{62, 60}, {61, 63}};
            }
        }
        
        return null;
    }

    internal static void UpdatePromotion(Board board, Move move, PIECE_COLOR color)
    {
        board.Layout[move.Target] = new Piece(color, move.Promotion);
        board.Layout[move.Source] = new Piece(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
    }

    internal static void UpdateDoublePawnPush(Board board, Move move, PIECE_COLOR color)
    {
        board.Layout[move.Target] = board.Layout[move.Source];
        board.Layout[move.Source] = new(PIECE_COLOR.NONE, PIECE_TYPE.NONE);
        if (color == PIECE_COLOR.BLACK)
        {
            board.EnPassantSq = move.Source + 8;
        } 
        else if (color == PIECE_COLOR.WHITE) 
        {
            board.EnPassantSq = move.Source - 8;
        }
    }
}