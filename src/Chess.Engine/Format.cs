using Chess.Engine.Structures;
using System.Text.Json;

namespace Chess.Engine;

public readonly record struct FenState(
    Piece[] Layout,
    PIECE_COLOR SideToMove,
    Castling CastlingRights,
    int EnPassantSquare,
    int HalfMoveClock,
    int FullMoveNumber);

public static class Format
{
    /// <summary>
    /// Converts FEN format to an array of Piece, that is used in internal game logic.
    /// Accepts both full FEN and board-only FEN.
    /// </summary>
    /// <param name="FEN">FEN as a String</param>
    /// <returns>A filled Piece[] array</returns>
    public static Piece[] ImportFEN(string FEN)
    {
        return ParseFEN(FEN).Layout;
    }

    public static FenState ParseFEN(string fen)
    {
        if (string.IsNullOrWhiteSpace(fen))
        {
            throw new ArgumentException("FEN cannot be empty.", nameof(fen));
        }

        var parts = fen.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is < 1 or > 6)
        {
            throw new FormatException("FEN must contain between 1 and 6 fields.");
        }

        var layout = ParsePiecePlacement(parts[0]);
        var sideToMove = parts.Length > 1 ? ParseSideToMove(parts[1]) : PIECE_COLOR.WHITE;
        var castling = parts.Length > 2 ? ParseCastling(parts[2]) : Castling.WhiteKingSide | Castling.WhiteQueenSide | Castling.BlackKingSide | Castling.BlackQueenSide;
        var enPassant = parts.Length > 3 ? ParseEnPassant(parts[3]) : -1;
        var halfMoveClock = parts.Length > 4 ? ParseNonNegativeInt(parts[4], "halfmove clock") : 0;
        var fullMoveNumber = parts.Length > 5 ? ParsePositiveInt(parts[5], "fullmove number") : 1;

        return new FenState(layout, sideToMove, castling, enPassant, halfMoveClock, fullMoveNumber);
    }

    public static Board ImportBoardFEN(string fen)
    {
        var state = ParseFEN(fen);
        return new Board(state.Layout)
        {
            SideToMove = state.SideToMove,
            CastlingRights = state.CastlingRights,
            EnPassantSq = state.EnPassantSquare,
            HalfMoveClock = state.HalfMoveClock,
            FullMoveNumber = state.FullMoveNumber
        };
    }

    public static string ExportFEN(Board board)
    {
        var piecePlacement = ExportPiecePlacement(board.Layout);
        var side = board.SideToMove == PIECE_COLOR.WHITE ? "w" : "b";
        var castling = ExportCastling(board.CastlingRights);
        var enPassant = board.EnPassantSq >= 0 ? IndexToSquare(board.EnPassantSq) : "-";
        return $"{piecePlacement} {side} {castling} {enPassant} {board.HalfMoveClock} {board.FullMoveNumber}";
    }

    /// <summary>
    /// Queries a local JSON file to extract a layout of pieces. Format "key: FEN".
    /// </summary>
    /// <param name="file">The path to the JSON file</param>
    /// <param name="key">The identifier - first value of JSON</param>
    /// <returns>FEN in a String format</returns>
    public static string GetFENJsonFile(string file, string key)
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
        Console.Write($"{'a',12}, {'b', 12}, {'c', 12}, {'d', 12}, {'e', 12}, {'f', 12}, {'g', 12}, {'h', 12} ");
        for (int i = 0; i < 64; i++)
        {
            if ((i & 7) == 0)
            {
                Console.WriteLine();
                Console.Write($"{8-(i >> 3)} ");
            }
            Console.Write($"{board[i].PC,5}_{board[i].PT,6}, ");
        }
        Console.WriteLine();
    }

    private static Piece[] ParsePiecePlacement(string placement)
    {
        var parsed = new Piece[64];
        int boardIndex = 0;
        int rank = 0;

        foreach (var symbol in placement)
        {
            if (symbol == '/')
            {
                if ((boardIndex & 7) != 0)
                {
                    throw new FormatException("Each FEN rank must describe exactly 8 squares.");
                }

                rank++;
                continue;
            }

            if (char.IsDigit(symbol))
            {
                int empty = symbol - '0';
                if (empty is < 1 or > 8)
                {
                    throw new FormatException("FEN empty-square count must be between 1 and 8.");
                }

                for (int k = 0; k < empty; k++)
                {
                    if (boardIndex >= 64)
                    {
                        throw new FormatException("FEN piece placement has too many squares.");
                    }

                    parsed[boardIndex++] = Piece.Empty;
                }
                continue;
            }

            if (boardIndex >= 64)
            {
                throw new FormatException("FEN piece placement has too many squares.");
            }

            parsed[boardIndex++] = ParsePiece(symbol);
        }

        if (rank != 7 || boardIndex != 64)
        {
            throw new FormatException("FEN piece placement must contain exactly 8 ranks and 64 squares.");
        }

        return parsed;
    }

    private static Piece ParsePiece(char symbol)
    {
        return symbol switch
        {
            'r' => new Piece(PIECE_COLOR.BLACK, PIECE_TYPE.ROOK),
            'n' => new Piece(PIECE_COLOR.BLACK, PIECE_TYPE.KNIGHT),
            'b' => new Piece(PIECE_COLOR.BLACK, PIECE_TYPE.BISHOP),
            'q' => new Piece(PIECE_COLOR.BLACK, PIECE_TYPE.QUEEN),
            'k' => new Piece(PIECE_COLOR.BLACK, PIECE_TYPE.KING),
            'p' => new Piece(PIECE_COLOR.BLACK, PIECE_TYPE.PAWN),
            'R' => new Piece(PIECE_COLOR.WHITE, PIECE_TYPE.ROOK),
            'N' => new Piece(PIECE_COLOR.WHITE, PIECE_TYPE.KNIGHT),
            'B' => new Piece(PIECE_COLOR.WHITE, PIECE_TYPE.BISHOP),
            'Q' => new Piece(PIECE_COLOR.WHITE, PIECE_TYPE.QUEEN),
            'K' => new Piece(PIECE_COLOR.WHITE, PIECE_TYPE.KING),
            'P' => new Piece(PIECE_COLOR.WHITE, PIECE_TYPE.PAWN),
            _ => throw new FormatException($"Invalid FEN piece symbol '{symbol}'.")
        };
    }

    private static PIECE_COLOR ParseSideToMove(string side)
    {
        return side switch
        {
            "w" => PIECE_COLOR.WHITE,
            "b" => PIECE_COLOR.BLACK,
            _ => throw new FormatException("FEN side-to-move must be 'w' or 'b'.")
        };
    }

    private static Castling ParseCastling(string castling)
    {
        if (castling == "-")
        {
            return Castling.None;
        }

        Castling rights = Castling.None;
        foreach (var c in castling)
        {
            rights |= c switch
            {
                'K' => Castling.WhiteKingSide,
                'Q' => Castling.WhiteQueenSide,
                'k' => Castling.BlackKingSide,
                'q' => Castling.BlackQueenSide,
                _ => throw new FormatException($"Invalid castling token '{c}' in FEN.")
            };
        }

        return rights;
    }

    private static int ParseEnPassant(string enPassant)
    {
        if (enPassant == "-")
        {
            return -1;
        }

        if (enPassant.Length != 2)
        {
            throw new FormatException("FEN en passant square must be '-' or a square like e3.");
        }

        return SquareToIndex(enPassant[0], enPassant[1]);
    }

    private static int ParseNonNegativeInt(string value, string fieldName)
    {
        if (!int.TryParse(value, out var result) || result < 0)
        {
            throw new FormatException($"FEN {fieldName} must be a non-negative integer.");
        }

        return result;
    }

    private static int ParsePositiveInt(string value, string fieldName)
    {
        if (!int.TryParse(value, out var result) || result < 1)
        {
            throw new FormatException($"FEN {fieldName} must be a positive integer.");
        }

        return result;
    }

    private static string ExportPiecePlacement(Piece[] layout)
    {
        var builder = new System.Text.StringBuilder(72);

        for (int rank = 0; rank < 8; rank++)
        {
            int emptyCount = 0;

            for (int file = 0; file < 8; file++)
            {
                int boardIndex = (rank * 8) + file;
                var piece = layout[boardIndex];

                if (piece.PC == PIECE_COLOR.NONE || piece.PT == PIECE_TYPE.NONE)
                {
                    emptyCount++;
                    continue;
                }

                if (emptyCount > 0)
                {
                    builder.Append((char)('0' + emptyCount));
                    emptyCount = 0;
                }

                builder.Append(PieceToFenChar(piece));
            }

            if (emptyCount > 0)
            {
                builder.Append((char)('0' + emptyCount));
            }

            if (rank < 7)
            {
                builder.Append('/');
            }
        }

        return builder.ToString();
    }

    private static char PieceToFenChar(Piece piece)
    {
        var symbol = piece.PT switch
        {
            PIECE_TYPE.PAWN => 'p',
            PIECE_TYPE.KNIGHT => 'n',
            PIECE_TYPE.BISHOP => 'b',
            PIECE_TYPE.ROOK => 'r',
            PIECE_TYPE.QUEEN => 'q',
            PIECE_TYPE.KING => 'k',
            _ => throw new FormatException("Cannot export an unknown piece type to FEN.")
        };

        return piece.PC == PIECE_COLOR.WHITE ? char.ToUpperInvariant(symbol) : symbol;
    }

    private static string ExportCastling(Castling rights)
    {
        if (rights == Castling.None)
        {
            return "-";
        }

        Span<char> buffer = stackalloc char[4];
        int idx = 0;
        if ((rights & Castling.WhiteKingSide) != 0) buffer[idx++] = 'K';
        if ((rights & Castling.WhiteQueenSide) != 0) buffer[idx++] = 'Q';
        if ((rights & Castling.BlackKingSide) != 0) buffer[idx++] = 'k';
        if ((rights & Castling.BlackQueenSide) != 0) buffer[idx++] = 'q';
        return new string(buffer[..idx]);
    }

    private static int SquareToIndex(char file, char rank)
    {
        if (file is < 'a' or > 'h' || rank is < '1' or > '8')
        {
            throw new FormatException($"Invalid square '{file}{rank}' in FEN.");
        }

        return ('8' - rank) * 8 + (file - 'a');
    }

    private static string IndexToSquare(int square)
    {
        if (square is < 0 or > 63)
        {
            throw new ArgumentOutOfRangeException(nameof(square), "Square index must be between 0 and 63.");
        }

        var file = (char)('a' + (square % 8));
        var rank = (char)('8' - (square / 8));
        return $"{file}{rank}";
    }
}

