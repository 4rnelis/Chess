using Chess.Engine.Structures;
using Xunit;

namespace Chess.Engine.Tests;

public class PerftTests
{

    private readonly Piece[] start = Format.ImportFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
    private readonly Piece[] kiwipete = Format.ImportFEN("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R");

    [Fact]
    public void Perft_StartPosition_Depth1()
    {
        var board = new Board(start);
        var stats = new PerftStats();
        Perft.Run(board, 1, stats);
        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");

        Assert.Equal(20, stats.Nodes);
    }

    [Fact]
    public void Perft_StartPosition_Depth2()
    {
        var board = new Board(start);
        var stats = new PerftStats();
        Perft.Run(board, 2, stats);
        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");

        Assert.Equal(400, stats.Nodes);
    }

    [Fact]
    public void Perft_StartPosition_Depth3()
    {
        var board = new Board(start);
        var stats = new PerftStats();
        Perft.Run(board, 3, stats);
        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");

        Assert.Equal(8902, stats.Nodes);
    }

    [Fact]
    public void Perft_StartPosition_Depth4()
    {
        var board = new Board(start);
        var stats = new PerftStats();
        Perft.Run(board, 4, stats);
        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");

        Assert.Equal(197281, stats.Nodes);
    }

    [Fact]
    public void Perft_StartPosition_Depth5()
    {
        var board = new Board(start);
        var stats = new PerftStats();
        Perft.Run(board, 5, stats);
        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");

        Assert.Equal(4865609, stats.Nodes);
    }


    [Fact]
    public void Perft_StartPosition_Depth6()
    {
        var board = new Board(start);
        var stats = new PerftStats();
        Perft.Run(board, 6, stats);
        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");

        Assert.Equal(119060324, stats.Nodes);
    }

    [Fact]
    public void Perft_KiwiPete_Depth1()
    {
        var board = new Board(kiwipete);
        var stats = new PerftStats();
        Perft.Run(board, 1, stats);
        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");

        Assert.Equal(48, stats.Nodes);
    }

    [Fact]
    public void Perft_KiwiPete_Depth2()
    {
        var board = new Board(kiwipete);
        var stats = new PerftStats();
        Perft.Run(board, 2, stats);
        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");

        Assert.Equal(2039, stats.Nodes);
    }

    [Fact]
    public void Perft_KiwiPete_Depth3()
    {
        var board = new Board(kiwipete);
        var stats = new PerftStats();
        Perft.Run(board, 3, stats);
        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");

        Assert.Equal(97862, stats.Nodes);
    }

    [Fact]
    public void Perft_KiwiPete_Depth4()
    {
        var board = new Board(kiwipete);
        var stats = new PerftStats();
        Perft.Run(board, 4, stats);

        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");
        Assert.Equal(4085603, stats.Nodes);
    }

    [Fact]
    public void Perft_KiwiPete_Depth5()
    {
        var board = new Board(kiwipete);
        var stats = new PerftStats();
        Perft.Run(board, 5, stats);

        Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");
        Assert.Equal(193690690, stats.Nodes);
    }

    // [Fact]
    // public void Perft_KiwiPete_Depth6()
    // {
    //     var board = new Board(kiwipete);
    //     var stats = new PerftStats();
    //     Perft.Run(board, 6, stats);

    //     Console.WriteLine($"Nodes: {stats.Nodes}, Captures: {stats.Captures}, EnPassant: {stats.EnPassant}, Castles: {stats.Castles}, Promotions: {stats.Promotions}");
    //     Assert.Equal(8031647685, stats.Nodes);
    // }
}
