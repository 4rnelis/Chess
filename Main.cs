using Chess.Logic;

Console.WriteLine("Hello, World!");

var board = Logic.ImportFEN(Logic.GetFENJsonFile("./resources/layouts.json", "1"));
Logic.PrintBoard(board);
