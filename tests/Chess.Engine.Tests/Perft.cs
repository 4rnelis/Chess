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
        List<Move> moves = MovesGenerator.GenerateAllMoves(board);

        foreach (Move move in moves)
        {
            // Skip if move leaves king in check
            var undo = board.MakeMove(move);
            if (undo == null)
                continue;

            // For depth 1, count move types at leaf nodes
            if (depth == 1)
            {
                if ((move.Flags & MOVE_FLAGS.Capture) != 0) stats.Captures++;
                if ((move.Flags & MOVE_FLAGS.EnPassant) != 0) stats.EnPassant++;
                if ((move.Flags & MOVE_FLAGS.Castling) != 0) stats.Castles++;
                if ((move.Flags & MOVE_FLAGS.Promotion) != 0) stats.Promotions++;
            }

            long subtreeNodes = Run(board, depth - 1, stats);
            nodes += subtreeNodes;

            board.UndoMove(move, undo.Value);
        }

        return nodes;
    }
}

