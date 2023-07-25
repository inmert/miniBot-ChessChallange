using System;
using ChessChallenge.API;

public class MyBot : IChessBot
{

    // Piece values: null, pawn, knight, bishop, rook, queen, king
    readonly int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
    readonly int initDepth = 3;
    Board board;
    ulong[] squareTable =
    {
        05662700134740480UL, 05673673944622080UL, 05673673945267200UL, 05679171503404800UL, 08499418828648960UL, 11303173479479040UL, 19747465729816320UL, 19736448970273280UL,
        02858945652339200UL, 02869940937712640UL, 02869940939013120UL, 02869941023225600UL, 05684690706117120UL, 08504938199461120UL, 19758482573900800UL, 22562194022085120UL,
        02858945652994560UL, 02869940939023360UL, 02875438581696000UL, 02875438582021120UL, 05690188433011200UL, 08504938199787520UL, 14134480515251200UL, 16932694488527360UL,
        00049714918863360UL, 00055191171916800UL, 00060688898805760UL, 00060688899132160UL, 02875438666237440UL, 08504938200117760UL, 14128982957432320UL, 14123442279559680UL,
        00049714918863360UL, 00055191171916800UL, 00060688898805760UL, 00060688899132160UL, 02875438666237440UL, 08504938200117760UL, 14128982957432320UL, 14123442279559680UL,
        02858945652994560UL, 02869940939023360UL, 02875438581696000UL, 02875438582021120UL, 05690188433011200UL, 08504938199787520UL, 14128982957112320UL, 16932694488527360UL,
        02858945652339200UL, 02869940937712640UL, 02869940939013120UL, 02869941023225600UL, 05684690706117120UL, 08499440641322240UL, 19758482573900800UL, 22562194022085120UL,
        05662700134740480UL, 05673673944622080UL, 05673673945267200UL, 05679171503404800UL, 08493921270510080UL, 11303173479479040UL, 19747465729816320UL, 19736448970273280UL
    };

    public Move Think(Board board, Timer timer)
    {
        this.board = board;
        bool isWhite = board.IsWhiteToMove;
        double bestValue = isWhite ? int.MinValue : int.MaxValue;
        double alpha = double.MinValue;
        double beta = double.MaxValue;
        Move bestMove = Move.NullMove;
        Console.WriteLine(MaterialAndPositionEvaluation());

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);

            double value = Minimax(initDepth, alpha, beta, !isWhite);

            board.UndoMove(move);

            if ((isWhite && value > bestValue) || (!isWhite && value < bestValue))
            {
                bestValue = value;
                bestMove = move;
            }

            if (isWhite)
            {
                alpha = Math.Max(alpha, bestValue);
            }
            else
            {
                beta = Math.Min(beta, bestValue);
            }
        }

        return bestMove;
    }

    public double Minimax(int depth, double alpha, double beta, bool maximizingPlayer)
    {
        if (depth == 0 || board.IsDraw() || board.IsInCheckmate())
            return MaterialAndPositionEvaluation();

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);

            double value = Minimax(depth - 1, alpha, beta, !maximizingPlayer);

            board.UndoMove(move);

            if (maximizingPlayer)
            {
                alpha = Math.Max(alpha, value);
                if (alpha >= beta)
                    break;
            }
            else
            {
                beta = Math.Min(beta, value);
                if (alpha >= beta)
                    break;
            }
        }

        return maximizingPlayer ? alpha : beta;
    }

    public double MaterialAndPositionEvaluation()
    {
        double evaluation = 0;

        for (int color = 0; color < 2; color++)
        {
            for (int piece = 1; piece < 7; piece++)
            {
                ulong bitboard = board.GetPieceBitboard((PieceType)piece, color == 0);
                while (bitboard != 0)
                {
                    int squareIndex = (int)Math.Log2(((bitboard ^ (bitboard - 1)) >> 1) + 1), f = squareIndex / 8, r = squareIndex % 8;
                    bitboard &= ~(1UL << squareIndex);
                    evaluation += (1 - 2 * color) * (pieceValues[piece] + (byte)(((squareTable[(color == 0 ? f : 7 - f) * 8 + r]) >> (piece * 8)) & 0xff));
                }
            }
        }

        return evaluation;
    }
}