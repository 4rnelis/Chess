using Chess.Engine.Structures;
using Chess.Engine.Logic;

namespace Chess.Engine.Tests;
public class PerftStats
{
    public long Nodes { get; set; } = 0;
    public long Captures { get; set; } = 0;
    public long EnPassant { get; set; } = 0;
    public long Castles { get; set; } = 0;
    public long Promotions { get; set; } = 0;
}

public static class Perft
{
    public static long Run(Board board, int depth, PerftStats stats)
    {
        if (depth == 0)
        {
            stats.Nodes++;
            return 1;
        }

        long nodes = 0;
        var layout = board.Layout;
        PIECE_COLOR sideToMove = board.SideToMove;
        Span<Move> moveBuffer = stackalloc Move[64];

        for (int sq = 0; sq < 64; sq++)
        {
            if (layout[sq].PC != sideToMove)
                continue;

            int moveCount = MovesGenerator.GetValidMovesCount(board, sq, moveBuffer);
            for (int i = 0; i < moveCount; i++)
            {
                Move move = moveBuffer[i];
                var undo = board.MakeMove(move);
                if (undo == null)
                    continue;

                if (depth == 1)
                {
                    stats.Nodes++;
                    if ((move.Flags & MOVE_FLAGS.Capture) != 0) stats.Captures++;
                    if ((move.Flags & MOVE_FLAGS.EnPassant) != 0) stats.EnPassant++;
                    if ((move.Flags & MOVE_FLAGS.Castling) != 0) stats.Castles++;
                    if ((move.Flags & MOVE_FLAGS.Promotion) != 0) stats.Promotions++;
                    nodes++;
                }
                else
                {
                    nodes += Run(board, depth - 1, stats);
                }

                board.UndoMove(move, undo.Value);
            }
        }

        return nodes;
    }
}
