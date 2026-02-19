using System.Runtime.CompilerServices;
using Chess.Engine.Structures;

namespace Chess.Engine.Logic;

/// <summary>
/// This class is used to contain a method of checking whether a specific square is under direct threat.
/// </summary>
public static class ThreatenedChecker
{
    private const int DirNorth = 0;
    private const int DirSouth = 1;
    private const int DirEast = 2;
    private const int DirWest = 3;
    private const int DirNorthEast = 4;
    private const int DirNorthWest = 5;
    private const int DirSouthEast = 6;
    private const int DirSouthWest = 7;

    private static readonly int[] DirectionOffsets = [-8, +8, +1, -1, -7, -9, +9, +7];
    private static readonly int[,] DistanceToEdge = GetDistanceToEdge();
    private static readonly int[][] KnightTargets = GetKnightTargets();
    private static readonly int[][] KingTargets = GetKingTargets();
    private static readonly int[][] PawnAttackersWhite = GetPawnAttackersWhite();
    private static readonly int[][] PawnAttackersBlack = GetPawnAttackersBlack();

    /// <summary>
    /// Checks if the square is under direct threat.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsThreatened(Board board, int position, PIECE_COLOR color)
    {
        PIECE_COLOR opposite = color == PIECE_COLOR.WHITE ? PIECE_COLOR.BLACK : PIECE_COLOR.WHITE;
        Piece[] layout = board.Layout;

        if (IsPawnThreatened(layout, position, color))
            return true;

        if (IsOrthogonallyThreatened(layout, position, opposite))
            return true;

        if (IsDiagonallyThreatened(layout, position, opposite))
            return true;

        if (IsKnightThreatened(layout, position, opposite))
            return true;

        return IsKingThreatened(layout, position, opposite);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPawnThreatened(Piece[] layout, int position, PIECE_COLOR color)
    {
        int[] attackers = color == PIECE_COLOR.WHITE
            ? PawnAttackersWhite[position]
            : PawnAttackersBlack[position];

        PIECE_COLOR opposite = color == PIECE_COLOR.WHITE ? PIECE_COLOR.BLACK : PIECE_COLOR.WHITE;

        for (int i = 0; i < attackers.Length; i++)
        {
            Piece piece = layout[attackers[i]];
            if (piece.PC == opposite && piece.PT == PIECE_TYPE.PAWN)
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOrthogonallyThreatened(Piece[] layout, int position, PIECE_COLOR opposite)
    {
        for (int dir = DirNorth; dir <= DirWest; dir++)
        {
            int offset = DirectionOffsets[dir];
            int max = DistanceToEdge[position, dir];

            for (int step = 1; step <= max; step++)
            {
                Piece piece = layout[position + offset * step];
                if (piece.PC == PIECE_COLOR.NONE)
                    continue;

                if (piece.PC == opposite && (piece.PT == PIECE_TYPE.ROOK || piece.PT == PIECE_TYPE.QUEEN))
                    return true;

                break;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDiagonallyThreatened(Piece[] layout, int position, PIECE_COLOR opposite)
    {
        for (int dir = DirNorthEast; dir <= DirSouthWest; dir++)
        {
            int offset = DirectionOffsets[dir];
            int max = DistanceToEdge[position, dir];

            for (int step = 1; step <= max; step++)
            {
                Piece piece = layout[position + offset * step];
                if (piece.PC == PIECE_COLOR.NONE)
                    continue;

                if (piece.PC == opposite && (piece.PT == PIECE_TYPE.BISHOP || piece.PT == PIECE_TYPE.QUEEN))
                    return true;

                break;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsKnightThreatened(Piece[] layout, int position, PIECE_COLOR opposite)
    {
        int[] targets = KnightTargets[position];
        for (int i = 0; i < targets.Length; i++)
        {
            Piece piece = layout[targets[i]];
            if (piece.PC == opposite && piece.PT == PIECE_TYPE.KNIGHT)
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsKingThreatened(Piece[] layout, int position, PIECE_COLOR opposite)
    {
        int[] targets = KingTargets[position];
        for (int i = 0; i < targets.Length; i++)
        {
            Piece piece = layout[targets[i]];
            if (piece.PC == opposite && piece.PT == PIECE_TYPE.KING)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the amount of squares to the edge for each direction
    /// </summary>
    /// <returns></returns>
    private static int[,] GetDistanceToEdge()
    {
        var result = new int[64, 8];
        for (int sq = 0; sq < 64; sq++)
        {
            int file = sq & 7;
            int rank = sq >> 3;

            int north = rank;
            int south = 7 - rank;
            int west = file;
            int east = 7 - file;

            result[sq, DirNorth] = north;
            result[sq, DirSouth] = south;
            result[sq, DirEast] = east;
            result[sq, DirWest] = west;
            result[sq, DirNorthEast] = Math.Min(north, east);
            result[sq, DirNorthWest] = Math.Min(north, west);
            result[sq, DirSouthEast] = Math.Min(south, east);
            result[sq, DirSouthWest] = Math.Min(south, west);
        }

        return result;
    }

    private static int[][] GetKnightTargets()
    {
        int[] fileOffsets = [+1, -1, +1, -1, +2, +2, -2, -2];
        int[] rankOffsets = [+2, +2, -2, -2, +1, -1, +1, -1];
        var result = new int[64][];

        for (int sq = 0; sq < 64; sq++)
        {
            int file = sq & 7;
            int rank = sq >> 3;
            int[] temp = new int[8];
            int count = 0;

            for (int i = 0; i < 8; i++)
            {
                int f = file + fileOffsets[i];
                int r = rank + rankOffsets[i];
                if ((uint)f < 8 && (uint)r < 8)
                {
                    temp[count++] = r * 8 + f;
                }
            }

            var targets = new int[count];
            Array.Copy(temp, targets, count);
            result[sq] = targets;
        }

        return result;
    }

    private static int[][] GetKingTargets()
    {
        int[] fileOffsets = [-1, -1, -1, 0, 0, +1, +1, +1];
        int[] rankOffsets = [-1, 0, +1, -1, +1, -1, 0, +1];
        var result = new int[64][];

        for (int sq = 0; sq < 64; sq++)
        {
            int file = sq & 7;
            int rank = sq >> 3;
            int[] temp = new int[8];
            int count = 0;

            for (int i = 0; i < 8; i++)
            {
                int f = file + fileOffsets[i];
                int r = rank + rankOffsets[i];
                if ((uint)f < 8 && (uint)r < 8)
                {
                    temp[count++] = r * 8 + f;
                }
            }

            var targets = new int[count];
            Array.Copy(temp, targets, count);
            result[sq] = targets;
        }

        return result;
    }

    private static int[][] GetPawnAttackersWhite()
    {
        var result = new int[64][];
        for (int sq = 0; sq < 64; sq++)
        {
            int file = sq & 7;
            int[] temp = new int[2];
            int count = 0;

            int left = sq - 9;
            if (file != 0 && (uint)left < 64)
                temp[count++] = left;

            int right = sq - 7;
            if (file != 7 && (uint)right < 64)
                temp[count++] = right;

            var targets = new int[count];
            Array.Copy(temp, targets, count);
            result[sq] = targets;
        }

        return result;
    }

    private static int[][] GetPawnAttackersBlack()
    {
        var result = new int[64][];
        for (int sq = 0; sq < 64; sq++)
        {
            int file = sq & 7;
            int[] temp = new int[2];
            int count = 0;

            int left = sq + 9;
            if (file != 7 && (uint)left < 64)
                temp[count++] = left;

            int right = sq + 7;
            if (file != 0 && (uint)right < 64)
                temp[count++] = right;

            var targets = new int[count];
            Array.Copy(temp, targets, count);
            result[sq] = targets;
        }

        return result;
    }
}
