using Chess.Engine.Structures;
using Chess.Engine.Logic;
using Chess.Engine;

Console.WriteLine("Hello, World!");

var layout = Format.ImportFEN(Format.GetFENJsonFile("./Engine/resources/layouts.json", "1"));
Board board = new Board(layout);
Format.PrintBoard(board.Layout);

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
    MoveMaker.MakeMove(board, move);
    Format.PrintBoard(board.Layout);
    Console.WriteLine($"Black mated: {MateChecker.CheckMate(board, PIECE_COLOR.BLACK)}");
    Console.WriteLine($"White mated: {MateChecker.CheckMate(board, PIECE_COLOR.WHITE)}");

}
