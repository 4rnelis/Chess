using System.Data.Common;
using System.Runtime.CompilerServices;
using Chess.Infrastructure;
using Microsoft.VisualBasic;

/// <summary>
/// This class is used to contain a method of checking whether a specific square is under direct threat
/// </summary>
public static class ThreatenedChecker
{
    /// <summary>
    /// Checks if the squate is under direct threat
    /// </summary>
    /// <param name="board"></param>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <returns>true/false</returns>
    public static bool IsThreatened(Board board, int position, PIECE_COLOR color)
    {
        return IsPawnThreatened(board, position, color) || 
            IsOrthogonallyThreatened(board, position, color) || 
            IsDiagonallyThreatened(board, position, color) ||
            IsKnightThreatened(board, position, color) || 
            IsKingThreatened(board, position, color);
    }

    private static bool IsPawnThreatened(Board board, int position, PIECE_COLOR color) {
        int possible_pawn_pos;
        Piece possible_pawn;
        switch (color)
        {
            case PIECE_COLOR.BLACK:
                {
                    if ((position & 7) != 7)
                    {
                        possible_pawn_pos = position + 9;
                        if ((uint)possible_pawn_pos < 64)
                        {
                            possible_pawn = board.Layout[possible_pawn_pos];
                            if (possible_pawn.PC == PIECE_COLOR.WHITE && possible_pawn.PT == PIECE_TYPE.PAWN)
                            {
                                return true;
                            }
                        }
                    }
                    if ((position & 7) != 0)
                    {
                        possible_pawn_pos = position + 7;
                        if ((uint)possible_pawn_pos < 64)
                        {
                            possible_pawn = board.Layout[possible_pawn_pos];
                            if (possible_pawn.PC == PIECE_COLOR.WHITE && possible_pawn.PT == PIECE_TYPE.PAWN)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            case PIECE_COLOR.WHITE:
                {
                    if ((position & 7) != 7)
                    {
                        possible_pawn_pos = position - 9;
                        if ((uint)possible_pawn_pos < 64)
                        {
                            possible_pawn = board.Layout[possible_pawn_pos];
                            if (possible_pawn.PC == PIECE_COLOR.BLACK && possible_pawn.PT == PIECE_TYPE.PAWN)
                            {
                                return true;
                            }
                        }
                    }
                    if ((position & 7) != 0)
                    {
                        possible_pawn_pos = position - 7;
                        if ((uint)possible_pawn_pos < 64)
                        {
                            possible_pawn = board.Layout[possible_pawn_pos];
                            if (possible_pawn.PC == PIECE_COLOR.BLACK && possible_pawn.PT == PIECE_TYPE.PAWN)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
        }
        return false;
    } 

    private static bool IsOrthogonallyThreatened(Board board, int position, PIECE_COLOR color) {
        
        // Source file
        int sf = position & 7;
        // Source rank
        int sr = position >> 3;
        int f, r, tp;
        Piece potentialRQ;

        int[] df = [-1, +1, 0, 0];
        int[] dr = [0, 0, -1, +1];

        // Iterate over each direction
        for (int dir = 0; dir < 4; dir++)
        {
            // file/rank = source f/r + a move in a direction
            f = sf + df[dir];
            r = sr + dr[dir];

            while ((uint)f < 8 && (uint)r < 8)
            {
                tp = r*8+f;
                potentialRQ = board.Layout[tp];
                if (potentialRQ.PC != PIECE_COLOR.NONE)
                {
                    if (potentialRQ.PC == Board.GetOppositeColor(color) && (potentialRQ.PT == PIECE_TYPE.QUEEN || potentialRQ.PT == PIECE_TYPE.ROOK))
                    {
                        return true;
                    }
                    break;
                }

                f += df[dir];
                r += dr[dir];
            } 
        }
        return false;
    } 

    private static bool IsDiagonallyThreatened(Board board, int position, PIECE_COLOR color) {
        
        // Source file
        int sf = position & 7;
        // Source rank
        int sr = position >> 3;
        int f, r, tp;
        Piece potentialRQ;

        int[] df = [+1, +1, -1, -1];
        int[] dr = [+1, -1, +1, -1];

        // Iterate over each direction
        for (int dir = 0; dir < 4; dir++)
        {
            // file/rank = source f/r + a move in a direction
            f = sf + df[dir];
            r = sr + dr[dir];

            while ((uint)f < 8 && (uint)r < 8)
            {
                tp = r*8+f;
                potentialRQ = board.Layout[tp];
                if (potentialRQ.PC != PIECE_COLOR.NONE)
                {
                    if (potentialRQ.PC == Board.GetOppositeColor(color) && (potentialRQ.PT == PIECE_TYPE.QUEEN || potentialRQ.PT == PIECE_TYPE.BISHOP))
                    {
                        return true;
                    }
                    break;
                }

                f += df[dir];
                r += dr[dir];
            } 
        }
        return false;
    }
    private static bool IsKnightThreatened(Board board, int position, PIECE_COLOR color) {

        List<Move> moves = [];
        // Source file
        int sf = position & 7;
        // Source rank
        int sr = position >> 3;
        int f, r, tp;
        Piece potentialKnight;

        int[] mf = [+1, -1, +1, -1, +2, +2, -2, -2];
        int[] mr = [+2, +2, -2, -2, +1, -1, +1, -1];

        for (int mv = 0; mv < 8; mv++)
        {
            f = sf + mf[mv];
            r = sr + mr[mv];

            if ((uint)f < 8 && (uint)r < 8) {
                tp = r*8+f;
                potentialKnight = board.Layout[tp];
                if (potentialKnight.PC != color)
                {
                    if (potentialKnight.PC == Board.GetOppositeColor(color) && potentialKnight.PT == PIECE_TYPE.KNIGHT) {
                        return true;
                    }
                } 
            }
        }
        return false;
    } 
    private static bool IsKingThreatened(Board board, int position, PIECE_COLOR color) {

        List<Move> moves = [];
        // Source file
        int sf = position & 7;
        // Source rank
        int sr = position >> 3;
        int f, r, tp;
        Piece potentialKing;

        int[] mf = [-1, -1, -1, 0, 0, +1, +1, +1];
        int[] mr = [-1, 0, +1, -1, +1, -1, 0, +1];

        for (int mv = 0; mv < 8; mv++)
        {
            f = sf + mf[mv];
            r = sr + mr[mv];

            if ((uint)f < 8 && (uint)r < 8) {
                tp = r*8+f;
                potentialKing = board.Layout[tp];
                if (potentialKing.PC != color)
                {
                    if (potentialKing.PC == Board.GetOppositeColor(color) && potentialKing.PT == PIECE_TYPE.KING) {
                        return true;
                    }
                } 
            }
        }
        return false;
    }
}