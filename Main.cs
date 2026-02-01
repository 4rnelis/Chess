using Chess.Structures;
using Chess.Logic;
using Chess;

Console.WriteLine("Hello, World!");

var layout = Format.ImportFEN(Format.GetFENJsonFile("./resources/layouts.json", "1"));
Board board = new Board(layout);
Format.PrintBoard(board.Layout);


var move = MoveMaker.ParseAndValidateUCI(board, "b7b5");
Console.WriteLine(move);

var moves = MovesGenerator.GetValidMoves(board, 24);
foreach (var m in moves)
{
    Console.WriteLine(m);
}

MoveMaker.MakeMove(board, move);
Format.PrintBoard(board.Layout);

moves = MovesGenerator.GetValidMoves(board, 24);
foreach (var m in moves)
{
    Console.WriteLine(m);
}

move = MoveMaker.ParseAndValidateUCI(board, "g2g4");
Console.WriteLine(move);

MoveMaker.MakeMove(board, move);
Format.PrintBoard(board.Layout);

moves = MovesGenerator.GetValidMoves(board, 39);
foreach (var m in moves)
{
    Console.WriteLine(m);
}

