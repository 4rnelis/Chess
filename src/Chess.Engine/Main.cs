using Chess.Engine.Structures;
using Chess.Engine.Logic;
using Chess.Engine;

Console.WriteLine("Hello, World!");

var layout = Format.ImportFEN(Format.GetFENJsonFile("./resources/layouts.json", "2"));
Board board = new Board(layout);
Format.PrintBoard(board.Layout);
Console.WriteLine(MoveMaker.SquareToUCI(board.KingPosition[0]));

while (true)
{
    var userInput = Console.ReadLine();
    if (userInput == null)
    {
        Console.WriteLine("No input from user read");
        continue;
    }
    Move? move = MoveMaker.ParseAndValidateUCI(board, userInput);
    if (move == null)
    {
        Console.WriteLine("Invalid move chosen!");
        continue;
    }
    Console.WriteLine(move);
    UndoState? undoState = MoveMaker.MakeMove(board, move);
    // Format.PrintBoard(board.Layout);
    // if (undoState.HasValue) {
    //     MoveMaker.UndoMove(board, move, undoState.Value);
    // }
    Format.PrintBoard(board.Layout);
    Console.WriteLine($"Black mated: {MateChecker.CheckMate(board, PIECE_COLOR.BLACK)}");
    Console.WriteLine($"White mated: {MateChecker.CheckMate(board, PIECE_COLOR.WHITE)}");
    
}
