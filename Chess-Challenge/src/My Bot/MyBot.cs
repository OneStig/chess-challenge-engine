using ChessChallenge.API;
using System;
using System.Linq;
using System.Collections;

public class MyBot : IChessBot
{
    int[] value_mg = { 82, 237, 365, 477, 1025, 12000 };
    int[] value_eg = { 94, 281, 297, 512, 936, 12000 };

    // value of pieces by game phase
    int[] value_gp = { 0, 1, 1, 2, 4, 0 };

    short[,] mg_table = {
        { // mg pawn table
            0,   0,   0,   0,   0,   0,  0,   0,
            98, 134,  61,  95,  68, 126, 34, -11,
            -6,   7,  26,  31,  65,  56, 25, -20,
            -14,  13,   6,  21,  23,  12, 17, -23,
            -27,  -2,  -5,  12,  17,   6, 10, -25,
            -26,  -4,  -4, -10,   3,   3, 33, -12,
            -35,  -1, -20, -23, -15,  24, 38, -22,
            0,   0,   0,   0,   0,   0,  0,   0
        },
        { // mg knight table
            -167, -89, -34, -49,  61, -97, -15, -107,
            -73, -41,  72,  36,  23,  62,   7,  -17,
            -47,  60,  37,  65,  84, 129,  73,   44,
            -9,  17,  19,  53,  37,  69,  18,   22,
            -13,   4,  16,  13,  28,  19,  21,   -8,
            -23,  -9,  12,  10,  19,  17,  25,  -16,
            -29, -53, -12,  -3,  -1,  18, -14,  -19,
            -105, -21, -58, -33, -17, -28, -19,  -23
        },
        { // mg bishop table
            -29,   4, -82, -37, -25, -42,   7,  -8,
            -26,  16, -18, -13,  30,  59,  18, -47,
            -16,  37,  43,  40,  35,  50,  37,  -2,
            -4,   5,  19,  50,  37,  37,   7,  -2,
            -6,  13,  13,  26,  34,  12,  10,   4,
            0,  15,  15,  15,  14,  27,  18,  10,
            4,  15,  16,   0,   7,  21,  33,   1,
            -33,  -3, -14, -21, -13, -12, -39, -21
        },
        { // mg rook table
            32,  42,  32,  51, 63,  9,  31,  43,
            27,  32,  58,  62, 80, 67,  26,  44,
            -5,  19,  26,  36, 17, 45,  61,  16,
            -24, -11,   7,  26, 24, 35,  -8, -20,
            -36, -26, -12,  -1,  9, -7,   6, -23,
            -45, -25, -16, -17,  3,  0,  -5, -33,
            -44, -16, -20,  -9, -1, 11,  -6, -71,
            -19, -13,   1,  17, 16,  7, -37, -26
        },
        { // mg queen table
            -28,   0,  29,  12,  59,  44,  43,  45,
            -24, -39,  -5,   1, -16,  57,  28,  54,
            -13, -17,   7,   8,  29,  56,  47,  57,
            -27, -27, -16, -16,  -1,  17,  -2,   1,
            -9, -26,  -9, -10,  -2,  -4,   3,  -3,
            -14,   2, -11,  -2,  -5,   2,  14,   5,
            -35,  -8,  11,   2,   8,  15,  -3,   1,
            -1, -18,  -9,  10, -15, -25, -31, -50
        },
        { // mg king table
            -65,  23,  16, -15, -56, -34,   2,  13,
            29,  -1, -20,  -7,  -8,  -4, -38, -29,
            -9,  24,   2, -16, -20,   6,  22, -22,
            -17, -20, -12, -27, -30, -25, -14, -36,
            -49,  -1, -27, -39, -46, -44, -33, -51,
            -14, -14, -22, -46, -44, -30, -15, -27,
            1,   7,  -8, -64, -43, -16,   9,   8,
            -15,  36,  12, -54,   8, -28,  24,  14
        }
    };
    short[,] eg_table = {
        { // eg pawn table
            0,   0,   0,   0,   0,   0,   0,   0,
            178, 173, 158, 134, 147, 132, 165, 187,
            94, 100,  85,  67,  56,  53,  82,  84,
            32,  24,  13,   5,  -2,   4,  17,  17,
            13,   9,  -3,  -7,  -7,  -8,   3,  -1,
            4,   7,  -6,   1,   0,  -5,  -1,  -8,
            13,   8,   8,  10,  13,   0,   2,  -7,
            0,   0,   0,   0,   0,   0,   0,   0
        },
        { // eg knight table
            -58, -38, -13, -28, -31, -27, -63, -99,
            -25,  -8, -25,  -2,  -9, -25, -24, -52,
            -24, -20,  10,   9,  -1,  -9, -19, -41,
            -17,   3,  22,  22,  22,  11,   8, -18,
            -18,  -6,  16,  25,  16,  17,   4, -18,
            -23,  -3,  -1,  15,  10,  -3, -20, -22,
            -42, -20, -10,  -5,  -2, -20, -23, -44,
            -29, -51, -23, -15, -22, -18, -50, -64
        },
        { // eg bishop table
            -14, -21, -11,  -8, -7,  -9, -17, -24,
            -8,  -4,   7, -12, -3, -13,  -4, -14,
            2,  -8,   0,  -1, -2,   6,   0,   4,
            -3,   9,  12,   9, 14,  10,   3,   2,
            -6,   3,  13,  19,  7,  10,  -3,  -9,
            -12,  -3,   8,  10, 13,   3,  -7, -15,
            -14, -18,  -7,  -1,  4,  -9, -15, -27,
            -23,  -9, -23,  -5, -9, -16,  -5, -17
        },
        { // eg rook table
            13, 10, 18, 15, 12,  12,   8,   5,
            11, 13, 13, 11, -3,   3,   8,   3,
            7,  7,  7,  5,  4,  -3,  -5,  -3,
            4,  3, 13,  1,  2,   1,  -1,   2,
            3,  5,  8,  4, -5,  -6,  -8, -11,
            -4,  0, -5, -1, -7, -12,  -8, -16,
            -6, -6,  0,  2, -9,  -9, -11,  -3,
            -9,  2,  3, -1, -5, -13,   4, -20
        },
        { // eg queen table
            -9,  22,  22,  27,  27,  19,  10,  20,
            -17,  20,  32,  41,  58,  25,  30,   0,
            -20,   6,   9,  49,  47,  35,  19,   9,
            3,  22,  24,  45,  57,  40,  57,  36,
            -18,  28,  19,  47,  31,  34,  39,  23,
            -16, -27,  15,   6,   9,  17,  10,   5,
            -22, -23, -30, -16, -16, -23, -36, -32,
            -33, -28, -22, -43,  -5, -32, -20, -41
        },
        { // eg king table
            -74, -35, -18, -18, -11,  15,   4, -17,
            -12,  17,  14,  17,  17,  38,  23,  11,
            10,  17,  23,  15,  20,  45,  44,  13,
            -8,  22,  24,  27,  26,  33,  26,   3,
            -18,  -4,  21,  24,  27,  23,   9, -11,
            -19,  -3,  11,  21,  23,  16,   7,  -9,
            -27, -11,   4,  13,  14,   4,  -5, -17,
            -53, -34, -21, -11, -28, -14, -24, -43
        }
    };

    public static long Encode(short first, short second, short third, short fourth)
    {
        first += 1000;
        second += 1000;
        third += 1000;
        fourth += 1000;

        long result = 0L;

        result |= (long)first << 48;      // Move first to the highest 16 bits
        result |= (long)second << 32;     // Move second to the next 16 bits
        result |= (long)third << 16;      // Move third to the next 16 bits
        result |= (long)fourth & 0xFFFFL; // Keep fourth in the lowest 16 bits

        return result;
    }

    public static short DecodeFirst(long encoded)
    {
        return (short)((encoded >> 48) - 1000);
    }

    public static short DecodeSecond(long encoded)
    {
        return (short)(((encoded >> 32) & 0xFFFF) - 1000);
    }

    public static short DecodeThird(long encoded)
    {
        return (short)(((encoded >> 16) & 0xFFFF) - 1000);
    }

    public static short DecodeFourth(long encoded)
    {
        return (short)((encoded & 0xFFFF) - 1000);
    }


    int depth = 3;

    struct Transposition
    {
        public ulong zobrist;
        public Move move;
        public int eval;

        public Transposition(ulong _zobrist, Move _move, int _eval)
        {
            zobrist = _zobrist;
            move = _move;
            eval = _eval;
        }
    }

    const int TTotal = (1 << 20);
    Transposition[] tt = new Transposition[TTotal];

    int tb_ind(int n)
    {
        return (7 - n / 8) * 8 + (n % 8);
    }

    int Eval(Board board)
    {
        Transposition cur = tt[board.ZobristKey % TTotal];
        if (cur.zobrist == board.ZobristKey)
        {
            return cur.eval;
        }

        int gamePhase = 0, mg_sum = 0, eg_sum = 0;

        PieceList[] all_pl = board.GetAllPieceLists();

        foreach (PieceList pl in all_pl)
        {
            for (int i = 0; i < pl.Count; i++)
            {
                Piece p = pl.GetPiece(i);

                int p_type_ind = (int)p.PieceType - 1;

                gamePhase += value_gp[p_type_ind];

                // if white flip board, otherwise dont (because arrays listed in POV of white)
                int ind = p.IsWhite ? tb_ind(p.Square.Index) : p.Square.Index;

                int neg = board.IsWhiteToMove == p.IsWhite ? 1 : -1;

                mg_sum += mg_table[p_type_ind, ind] + value_mg[p_type_ind] * neg;
                eg_sum += mg_table[p_type_ind, ind] + value_mg[p_type_ind] * neg;

                // Console.WriteLine("piece " + p.ToString() + " " + p.Square.ToString() + " " + (p.IsWhite ? this_value : this_value * -1));
            }
        }

        gamePhase = Math.Min(gamePhase, 24);

        int sum = (mg_sum * gamePhase + eg_sum * (24 - gamePhase)) / 24;

        cur = new Transposition(board.ZobristKey, Move.NullMove, sum);

        return sum;
    }

    public int Negamax(Board board, int depth, int alpha, int beta)
    {
        if (board.IsInCheckmate())
        {
            return -100000;
        }

        if (depth == 0)
        {
            return Eval(board);
        }

        int bestValue = int.MinValue;

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            int value = -Negamax(board, depth - 1, -beta, -alpha);
            board.UndoMove(move);

            bestValue = Math.Max(bestValue, value);
            alpha = Math.Max(alpha, value);

            if (alpha >= beta)
            {
                break;
            }
        }

        return bestValue;
    }

    public Move Think(Board board, Timer timer)
    {
        ArrayList encoded = new ArrayList();

        for (int i = 0; i < 6; i++) {
            for (int j = 0; j < 64; j++) {
                if (j % 4 == 0) {
                    encoded.Add(Encode(mg_table[i, j], mg_table[i, j + 1], mg_table[i, j + 2], mg_table[i, j + 3]));
                }
            }
        }
        
        string result = string.Join(", ", encoded.Cast<long>());
        Console.WriteLine(result);

        foreach (long l in encoded) {
            Console.WriteLine(DecodeFirst(l) + " " + DecodeSecond(l) + " " +
                                DecodeThird(l) + " " + DecodeFourth(l) + " ");
        }

        // Console.WriteLine("Current Board: " + Eval(board));
        Move[] allMoves = board.GetLegalMoves();

        Move moveToPlay = allMoves[0];

        int best_eval = int.MinValue;

        foreach (Move move in allMoves)
        {
            board.MakeMove(move);
            int move_eval = -Negamax(board, depth, int.MinValue, int.MaxValue);
            board.UndoMove(move);

            // Console.WriteLine(move.ToString() + " " + move_eval);

            if (move_eval > best_eval)
            {
                moveToPlay = move;
                best_eval = move_eval;
            }
        }

        // Console.WriteLine("Selected move: " + moveToPlay.ToString() + " " + best_eval);

        return moveToPlay;
    }
}