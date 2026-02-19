using System.Runtime.CompilerServices;
using Chess.Engine.Structures;

namespace Chess.Engine.Logic;

internal static class MoveMaker
{
    private static readonly Piece WhitePawn = new(PIECE_COLOR.WHITE, PIECE_TYPE.PAWN);
    private static readonly Piece BlackPawn = new(PIECE_COLOR.BLACK, PIECE_TYPE.PAWN);
    private static readonly Castling[] ClearRookCastlingMaskBySquare = BuildRookMaskTable();

    /// <summary>
    /// Takes a UCI string as an input, parses the coordinates to a 1D array, and checks if the requested move is valid.
    /// If yes, return the Move object for further logic.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="UCI"></param>
    /// <returns></returns>
    public static Move? ParseAndValidateUCI(Board board, string UCI)
    {
        int from = ParseSquare(UCI[0], UCI[1]);
        int to = ParseSquare(UCI[2], UCI[3]);

        Span<Move> moves = stackalloc Move[64];
        int moveCount = MovesGenerator.GetValidMovesCount(board, from, moves);

        // Check the promotion
        if (UCI.Length == 5)
        {
            PIECE_TYPE promotion = ParsePromotion(UCI[4]);
            for (int i = 0; i < moveCount; i++)
            {
                Move move = moves[i];
                if (move.Source == from && move.Target == to && move.Promotion == promotion)
                {
                    return move;
                }
            }
            return null;
        }

        Move? found = null;
        for (int i = 0; i < moveCount; i++)
        {
            Move move = moves[i];
            if (move.Source == from && move.Target == to)
            {
                if (found.HasValue)
                {
                    return null;
                }
                found = move;
            }
        }

        return found;
    }

    public static string ToUCI(Move move) {
        return SquareToUCI(move.Source) + SquareToUCI(move.Target);
    }

    public static int ParseSquare(char fc, char rc)
    {
        return ('8'-rc)*8+(fc-'a');
    }

    public static string SquareToUCI(int square)
    {
        char file = (char)('a' + (square % 8));
        char rank = (char)('8' - (square / 8));
        return $"{file}{rank}";
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
    /// Main function for making the move logic
    /// </summary>
    /// <param name="board"></param>
    /// <param name="move"></param>
    /// <returns></returns>
    internal static UndoState? MakeMove(Board board, Move move)
    {
        PIECE_COLOR color = board.Layout[move.Source].PC;
        PIECE_TYPE type = board.Layout[move.Source].PT;
        PIECE_TYPE capturedType = board.Layout[move.Target].PT; // Save for Rook castling rights checking

        // 1. Check validity
        // 2. Store minimal previous state for a fast undo in case of illegal move
        Piece prevSource = board.Layout[move.Source];
        Piece prevTarget = board.Layout[move.Target];
        PIECE_COLOR prevSide = board.SideToMove;
        Castling prevCastling = board.CastlingRights;
        int prevEnPassant = board.EnPassantSq;
        int prevKingBlack = board.KingPosition[0];
        int prevKingWhite = board.KingPosition[1];

        // 3. Make the move
        CastlingUndo undoCastling = UpdateMove(board, move, color, type, capturedType);

        // 4. Check if king is threatened. Should add King square caching
        if (ThreatenedChecker.IsThreatened(board, board.KingPosition[(int)color], color))
        {
            UndoMoveFast(board, move, prevSource, prevTarget, prevSide, prevCastling, prevEnPassant, prevKingBlack, prevKingWhite, undoCastling);
            return null;
        }

        // 5. Create full undo state only for legal moves
        UndoState undoState = new(prevSource, prevTarget, prevSide, prevCastling, prevEnPassant, prevKingBlack, prevKingWhite)
        {
            CastlingUndo = undoCastling
        };

        board.UndoState = undoState;

        board.SideToMove = prevSide == PIECE_COLOR.WHITE ? PIECE_COLOR.BLACK : PIECE_COLOR.WHITE;
        return undoState;
    }

    /// <summary>
    /// Method to undo the move without requiring an UndoState object
    /// </summary>
    /// <param name="board"></param>
    /// <param name="move"></param>
    /// <param name="prevSource"></param>
    /// <param name="prevTarget"></param>
    /// <param name="prevSide"></param>
    /// <param name="prevCastling"></param>
    /// <param name="prevEnPassant"></param>
    /// <param name="prevKingBlack"></param>
    /// <param name="prevKingWhite"></param>
    /// <param name="undoCastling"></param>
    private static void UndoMoveFast(
        Board board,
        Move move,
        Piece prevSource,
        Piece prevTarget,
        PIECE_COLOR prevSide,
        Castling prevCastling,
        int prevEnPassant,
        int prevKingBlack,
        int prevKingWhite,
        CastlingUndo undoCastling)
    {
        RestoreBoardState(board, move, prevSource, prevTarget, prevSide, prevCastling, prevEnPassant, prevKingBlack, prevKingWhite, undoCastling);
    }

    internal static void UndoMove(Board board, Move move, UndoState undoState)
    {
        RestoreBoardState(
            board,
            move,
            undoState.Source,
            undoState.Target,
            undoState.SideToMove,
            undoState.PrevCastlingRights,
            undoState.PrevEnPassant,
            undoState.PrevKingBlack,
            undoState.PrevKingWhite,
            undoState.CastlingUndo);
    }

    /// <summary>
    /// The logic for updating the state during a move
    /// </summary>
    /// <param name="board"></param>
    /// <param name="move"></param>
    /// <param name="color"></param>
    /// <param name="type"></param>
    /// <param name="capturedType"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CastlingUndo UpdateMove(Board board, Move move, PIECE_COLOR color, PIECE_TYPE type, PIECE_TYPE capturedType)
    {
        CastlingUndo undoCastling = default;
        var layout = board.Layout;
        MOVE_FLAGS flags = move.Flags;

        if ((flags & MOVE_FLAGS.Castling) != 0)
        {
            undoCastling = UpdateCastling(board, move, color);
            board.EnPassantSq = -1;
        }
        else if ((flags & MOVE_FLAGS.Promotion) != 0)
        {
            layout[move.Target] = new Piece(color, move.Promotion);
            layout[move.Source] = Piece.Empty;
            board.EnPassantSq = -1;
        }
        else
        {
            // Remove the captured pawn from enPassant
            if ((flags & MOVE_FLAGS.EnPassant) != 0)
            {
                if (color == PIECE_COLOR.BLACK)
                {
                    layout[move.Target-8] = Piece.Empty;
                }
                else
                {
                    layout[move.Target+8] = Piece.Empty;
                }
            }

            layout[move.Target] = layout[move.Source];
            layout[move.Source] = Piece.Empty;

            if ((flags & MOVE_FLAGS.DoublePawnPush) != 0)
            {
                board.EnPassantSq = color == PIECE_COLOR.BLACK ? move.Source + 8 : move.Source - 8;
            }
            else
            {
                board.EnPassantSq = -1;
            }
        }
        if (type == PIECE_TYPE.KING && (flags & MOVE_FLAGS.Castling) == 0)
        {
            board.KingPosition[(int)color] = move.Target;
            board.CastlingRights &= color == PIECE_COLOR.BLACK
                ? ~(Castling.BlackKingSide | Castling.BlackQueenSide)
                : ~(Castling.WhiteKingSide | Castling.WhiteQueenSide);
        }
        if (type == PIECE_TYPE.ROOK)
        {
            board.CastlingRights &= ~ClearRookCastlingMaskBySquare[move.Source];
        }
        if (capturedType == PIECE_TYPE.ROOK)
        {
            board.CastlingRights &= ~ClearRookCastlingMaskBySquare[move.Target];
        }
        return undoCastling;
    }

    internal static CastlingUndo UpdateCastling(Board board, Move move, PIECE_COLOR color)
    {
        CastlingUndo undo = default;
        undo.IsValid = true;
        var layout = board.Layout;

        // Remove the flags and switch depending which side/color
        if (color == PIECE_COLOR.BLACK)
        {
            board.CastlingRights &= ~Castling.BlackKingSide;
            board.CastlingRights &= ~Castling.BlackQueenSide;
            
            if (move.Source == 0 || move.Target == 0)
            {
                // Move King and Rook
                layout[2] = layout[4];
                layout[4] = Piece.Empty;
                layout[3] = layout[0];
                layout[0] = Piece.Empty;
                // Update cached king position
                board.KingPosition[0] = 2;
                // Save their moves
                undo.KingFrom = 2;
                undo.KingTo = 4;
                undo.RookFrom = 3;
                undo.RookTo = 0;
                return undo;
            }
            if (move.Source == 7 || move.Target == 7)
            {
                layout[6] = layout[4];
                layout[4] = Piece.Empty;
                layout[5] = layout[7];
                layout[7] = Piece.Empty;
                board.KingPosition[0] = 6;
                undo.KingFrom = 6;
                undo.KingTo = 4;
                undo.RookFrom = 5;
                undo.RookTo = 7;
                return undo;
            }
        }

        if (color == PIECE_COLOR.WHITE)
        {
            board.CastlingRights &= ~Castling.WhiteKingSide;
            board.CastlingRights &= ~Castling.WhiteQueenSide;
            
            if (move.Source == 56 || move.Target == 56)
            {
                layout[58] = layout[60];
                layout[60] = Piece.Empty;
                layout[59] = layout[56];
                layout[56] = Piece.Empty;
                board.KingPosition[1] = 58;
                undo.KingFrom = 58;
                undo.KingTo = 60;
                undo.RookFrom = 59;
                undo.RookTo = 56;
                return undo;
            }
            if (move.Source == 63 || move.Target == 63)
            {
                layout[62] = layout[60];
                layout[60] = Piece.Empty;
                layout[61] = layout[63];
                layout[63] = Piece.Empty;
                board.KingPosition[1] = 62;
                undo.KingFrom = 62;
                undo.KingTo = 60;
                undo.RookFrom = 61;
                undo.RookTo = 63;
                return undo;
            }
        }
        
        undo.IsValid = false;
        return undo;
    }

    /// <summary>
    /// Indicate which position of the rooks are resposible for each castling right
    /// </summary>
    /// <returns></returns>
    private static Castling[] BuildRookMaskTable()
    {
        var table = new Castling[64];
        table[0] = Castling.BlackQueenSide;
        table[7] = Castling.BlackKingSide;
        table[56] = Castling.WhiteQueenSide;
        table[63] = Castling.WhiteKingSide;
        return table;
    }

    /// <summary>
    /// Apply undo logic onto board
    /// </summary>
    /// <param name="board"></param>
    /// <param name="move"></param>
    /// <param name="sourcePiece"></param>
    /// <param name="targetPiece"></param>
    /// <param name="sideToMove"></param>
    /// <param name="castlingRights"></param>
    /// <param name="enPassantSq"></param>
    /// <param name="prevKingBlack"></param>
    /// <param name="prevKingWhite"></param>
    /// <param name="undoCastling"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RestoreBoardState(
        Board board,
        Move move,
        Piece sourcePiece,
        Piece targetPiece,
        PIECE_COLOR sideToMove,
        Castling castlingRights,
        int enPassantSq,
        int prevKingBlack,
        int prevKingWhite,
        CastlingUndo undoCastling)
    {
        board.CastlingRights = castlingRights;
        board.EnPassantSq = enPassantSq;
        board.KingPosition[0] = prevKingBlack;
        board.KingPosition[1] = prevKingWhite;
        board.SideToMove = sideToMove;

        var layout = board.Layout;
        MOVE_FLAGS flags = move.Flags;

        if ((flags & MOVE_FLAGS.Castling) != 0)
        {
            if (!undoCastling.IsValid)
                throw new Exception("CastlingUndo is invalid in UndoMove. Something is wrong with castling logic!");

            // Reset castling positions
            layout[undoCastling.KingTo] = layout[undoCastling.KingFrom];
            layout[undoCastling.KingFrom] = Piece.Empty;
            layout[undoCastling.RookTo] = layout[undoCastling.RookFrom];
            layout[undoCastling.RookFrom] = Piece.Empty;
            return;
        }

        // Uncapture the pawn from EnPassant capture
        if ((flags & MOVE_FLAGS.EnPassant) != 0)
        {
            int capturedPawnSquare = sourcePiece.PC == PIECE_COLOR.BLACK ? move.Target - 8 : move.Target + 8;
            layout[capturedPawnSquare] = sourcePiece.PC == PIECE_COLOR.BLACK ? WhitePawn : BlackPawn;
        }

        layout[move.Target] = targetPiece;
        layout[move.Source] = sourcePiece;
    }
}
