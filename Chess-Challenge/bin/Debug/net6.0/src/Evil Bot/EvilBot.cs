using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        readonly int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };
        readonly int initDepth = 3;

        ulong[,] squareTable = new ulong[8, 8]
        {
        { 22119922401330UL, 11167756454450UL, 11167756457010UL, 194198901810UL, 194198901810UL, 11167756457010UL, 11167756454450UL, 22119922401330UL },
        { 22162788846180UL, 11210706787940UL, 11210706793060UL, 215590515300UL, 215590515300UL, 11210706793060UL, 11210706787940UL, 22162788846180UL },
        { 22162788848700UL, 11210706793020UL, 11232181959750UL, 237066010960UL, 237066010960UL, 11232181959750UL, 11210706793020UL, 22162788848700UL },
        { 22184263685175UL, 11210707121975UL, 11232181961020UL, 237066012235UL, 237066012235UL, 11232181961020UL, 11210707121975UL, 22184263685175UL },
        { 33200854799410UL, 22205823070770UL, 22227298566450UL, 11232182289990UL, 11232182289990UL, 22227298566450UL, 22205823070770UL, 33179379962930UL },
        { 44153021404215UL, 33222414841645UL, 33222414842920UL, 33222414844210UL, 33222414844210UL, 33222414842920UL, 33200940005165UL, 44153021404215UL },
        { 77138538007095UL, 77181572554300UL, 55212814512700UL, 55191339677470UL, 55191339677470UL, 55191339676220UL, 77181572554300UL, 77138538007095UL },
        { 77095503790130UL, 88133570398770UL, 66143337845810UL, 55169696404530UL, 55169696404530UL, 66143337845810UL, 88133570398770UL, 77095503790130UL }
        };

        public Move Think(Board board, Timer timer)
        {
            int bestValue = board.IsWhiteToMove ? int.MinValue : int.MaxValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            Move bestMove = Move.NullMove;

            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);

                int value = Minimax(board, initDepth, alpha, beta, !board.IsWhiteToMove);
                board.UndoMove(move);

                if ((board.IsWhiteToMove && value > bestValue) || (!board.IsWhiteToMove && value < bestValue))
                {
                    bestValue = value;
                    bestMove = move;
                }

                if (board.IsWhiteToMove)
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


        public int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0)
            {
                return MaterialAndPositionEvaluation(board);
            }

            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);

                int value = 0;

                if (maximizingPlayer)
                {
                    value = Minimax(board, depth - 1, alpha, beta, false);
                    alpha = Math.Max(alpha, value);
                }
                else
                {
                    value = Minimax(board, depth - 1, alpha, beta, true);
                    beta = Math.Min(beta, value);
                }

                board.UndoMove(move);

                // Alpha-Beta Pruning
                if (alpha >= beta)
                    break;
            }

            return maximizingPlayer ? alpha : beta;
        }

        // PS EVAL FUNCTION

        public int MaterialAndPositionEvaluation(Board board)
        {
            int evaluation = 0;

            for (int color = 0; color < 2; color++)
            {
                foreach (PieceType pieceType in Enum.GetValues(typeof(PieceType)))
                {
                    ulong bitboard = board.GetPieceBitboard(pieceType, color == 0);
                    int pieceValue = color == 0 ? pieceValues[(int)pieceType] : -pieceValues[(int)pieceType];

                    while (bitboard != 0)
                    {
                        int squareIndex = BitScanReverse(bitboard);
                        bitboard &= ~(1UL << squareIndex); // Clear the least significant bit

                        evaluation += pieceValue + GetSquareTableEvaluation(pieceType, squareIndex, color);
                    }
                }
            }

            return evaluation;
        }

        // BitScanReverse method to find the index of the most significant bit set to 1 in an integer
        private int BitScanReverse(ulong bitboard)
        {
            int index = 0;

            while (bitboard != 0)
            {
                bitboard >>= 1;
                index++;
            }

            return index - 1;
        }

        private int GetSquareTableEvaluation(PieceType pieceType, int squareIndex, int color)
        {
            if (pieceType == PieceType.None) return 0;
            int file = squareIndex % 8; // Get the file (column) index
            int rank = squareIndex / 8; // Get the rank (row) index

            ulong val = (color == 0 ? squareTable[rank, file] : squareTable[7 - rank, file]);

            return ((byte)((val >> ((int)(pieceType - 1) * 8)) & 0xff) * (color == 0 ? 1 : -1)) - 50;
        }


    }
}