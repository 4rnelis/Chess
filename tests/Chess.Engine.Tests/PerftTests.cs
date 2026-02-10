using Chess.Engine.Structures;
using Xunit;

namespace Chess.Engine.Tests;

public class PerftTests
{

    private readonly Piece[] layout = Format.ImportFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");

    [Fact]
    public void Perft_StartPosition_Depth1()
    {
        var board = new Board(layout);
        Assert.Equal(20, Perft.Run(board, 1));
    }

    [Fact]
    public void Perft_StartPosition_Depth2()
    {
        var board = new Board(layout);
        Assert.Equal(400, Perft.Run(board, 2));
    }

    [Fact]
    public void Perft_StartPosition_Depth3()
    {
        var board = new Board(layout);
        Assert.Equal(8902, Perft.Run(board, 3));
    }

    [Fact]
    public void Perft_StartPosition_Depth4()
    {
        var board = new Board(layout);
        Assert.Equal(197281, Perft.Run(board, 4));
    }
}
