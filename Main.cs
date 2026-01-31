using Chess.Infrastructure;
using Chess.Logic;

Console.WriteLine("Hello, World!");

var layout = Logic.ImportFEN(Logic.GetFENJsonFile("./resources/layouts.json", "1"));
Board board = new Board(layout);
Logic.PrintBoard(board.Layout);

var moves = board.GetValidMoves(10);
foreach (var m in moves)
{
    Console.WriteLine($"{m.Source} {m.Target} {m.Flags}");
}
