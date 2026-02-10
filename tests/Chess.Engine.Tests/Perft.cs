using Chess.Engine.Structures;
using Chess.Engine.Logic;

namespace Chess.Engine.Tests;
public static class Perft
{
    public static long Run(Board board, int depth)
    {
        Console.WriteLine($"[PERFT] Depth: {depth}");
        if (depth == 0)
            return 1;

        long nodes = 0;

        for (int sq = 0; sq < 64; sq++)
        {
            if (board.Layout[sq].PC != board.SideToMove)
                continue;

            var moves = MovesGenerator.GetValidMoves(board, sq);

            foreach (var move in moves)
            {
                var undo = board.MakeMove(move);
                if (undo == null)
                    continue;

                nodes += Run(board, depth - 1);

                board.UndoMove(move, undo.Value);
            }
        }

        return nodes;
    }
}
