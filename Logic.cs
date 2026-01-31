using Chess.Infrastructure;
using System.Text.Json;

namespace Chess.Logic;

using System.Runtime.Intrinsics.X86;
using System.Security;


public static class Logic
{
    /// <summary>
    /// Converts FEN format to an array of Piece, that is used in internal game logic.
    /// </summary>
    /// <param name="FEN">FEN as a String</param>
    /// <returns>A filled Piece[] array</returns>
    public static Piece[] ImportFEN(String FEN) 
    {
        Piece[] parsed = new Piece[64];
        int i = 0;
        while (i < 64) {
            foreach (char e in FEN) {
                switch (e) {
                    case 'r':
                        parsed[i] = new Piece(pc: PIECE_COLOR.BLACK, pt: PIECE_TYPE.ROOK);
                        i++;
                        break;
                    case 'n':
                        parsed[i] = new Piece(pc: PIECE_COLOR.BLACK, pt: PIECE_TYPE.KNIGHT);
                        i++;
                        break;
                    case 'b':
                        parsed[i] = new Piece(pc: PIECE_COLOR.BLACK, pt: PIECE_TYPE.BISHOP);
                        i++;
                        break;
                    case 'q':
                        parsed[i] = new Piece(pc: PIECE_COLOR.BLACK, pt: PIECE_TYPE.QUEEN);
                        i++;
                        break;
                    case 'k':
                        parsed[i] = new Piece(pc: PIECE_COLOR.BLACK, pt: PIECE_TYPE.KING);
                        i++;
                        break;
                    case 'p':
                        parsed[i] = new Piece(pc: PIECE_COLOR.BLACK, pt: PIECE_TYPE.PAWN);
                        i++;
                        break;
                    case 'R':
                        parsed[i] = new Piece(pc: PIECE_COLOR.WHITE, pt: PIECE_TYPE.ROOK);
                        i++;
                        break;
                    case 'N':
                        parsed[i] = new Piece(pc: PIECE_COLOR.WHITE, pt: PIECE_TYPE.KNIGHT);
                        i++;
                        break;
                    case 'B':
                        parsed[i] = new Piece(pc: PIECE_COLOR.WHITE, pt: PIECE_TYPE.BISHOP);
                        i++;
                        break;
                    case 'Q':
                        parsed[i] = new Piece(pc: PIECE_COLOR.WHITE, pt: PIECE_TYPE.QUEEN);
                        i++;
                        break;
                    case 'K':
                        parsed[i] = new Piece(pc: PIECE_COLOR.WHITE, pt: PIECE_TYPE.KING);
                        i++;
                        break;
                    case 'P':
                        parsed[i] = new Piece(pc: PIECE_COLOR.WHITE, pt: PIECE_TYPE.PAWN);
                        i++;
                        break;
                    case '/':
                        continue;
                    default:
                        for (int k = 0; k < e-'0'; k++)
                        {
                            parsed[i] = new Piece(pc: PIECE_COLOR.NONE, pt: PIECE_TYPE.NONE);
                            i++;
                        }
                        break;
                }
            }
        }
        return parsed;
    }

    /// <summary>
    /// Queries a local JSON file to extract a layout of pieces. Format "key: FEN".
    /// </summary>
    /// <param name="file">The path to the JSON file</param>
    /// <param name="key">The identifier - first value of JSON</param>
    /// <returns>FEN in a String format</returns>
    public static String GetFENJsonFile(String file, String key)
    {
        var json = File.ReadAllText(file);

        var layouts = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                    ?? throw new InvalidOperationException("Invalid JSON");

        if (layouts.TryGetValue(key, out var fen))
        {
            return fen;
        } else throw new Exception($"Layout {key} does not exist in {file}");
    }

    /// <summary>
    /// Prints the disposition of pieces on the board.
    /// </summary>
    /// <param name="board">The array of pieces</param>
    public static void PrintBoard(Piece[] board)
    {
        for (int i = 0; i < 64; i++)
        {
            if ((i & 7) == 0)
            {
                Console.WriteLine();
            }
            Console.Write($"{board[i].PC}_{board[i].PT}, ");
        }
        Console.WriteLine();
    }
}

