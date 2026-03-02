using Chess.Engine.Structures;
using Xunit;

namespace Chess.Engine.Tests;

public sealed class FormatFenTests
{
    [Fact]
    public void ParseFEN_FullFen_ParsesAllFields()
    {
        const string fen = "rnbqkbnr/pppp1ppp/8/4p3/3P4/8/PPP1PPPP/RNBQKBNR b KQkq d3 0 2";

        var state = Format.ParseFEN(fen);

        Assert.Equal(PIECE_COLOR.BLACK, state.SideToMove);
        Assert.Equal(Castling.WhiteKingSide | Castling.WhiteQueenSide | Castling.BlackKingSide | Castling.BlackQueenSide, state.CastlingRights);
        Assert.Equal(43, state.EnPassantSquare); // d3
        Assert.Equal(0, state.HalfMoveClock);
        Assert.Equal(2, state.FullMoveNumber);
    }

    [Fact]
    public void ExportFEN_BoardFromFullFen_RoundTrips()
    {
        const string fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 3 9";
        var board = Format.ImportBoardFEN(fen);

        var exported = Format.ExportFEN(board);

        Assert.Equal(fen, exported);
    }
}
