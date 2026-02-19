using Chess.Engine.Structures;

namespace Chess.Engine.Logic;

public static class MateChecker
{
    /// <summary>
    /// Checks if a color is mated. Iterates over pieces and tries to find a legal move.
    /// </summary>
    /// <param name="board"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static bool CheckMate(Board board, PIECE_COLOR color)
    {
        // Console.WriteLine($"[MateChecker] Init for {color}");
        for (int i = 0; i < 64; i++)
        {
            if (board.Layout[i].PC == color)
            {
                // Console.WriteLine($"[MateChecker] Checking piece: {board.Layout[i].PT}");
                var moves = MovesGenerator.GetValidMoves(board, i);
                foreach (var move in moves)
                {
                    // Console.WriteLine($"[MateChecker] Making move {move}");
                    var undoState = MoveMaker.MakeMove(board, move);
                    if (undoState.HasValue)
                    {
                        // Console.WriteLine($"[MateChecker] Valid move: {move}");
                        MoveMaker.UndoMove(board, move, undoState.Value);

                        return false;
                    }
                }
            }
        }
        return true;
    }
}