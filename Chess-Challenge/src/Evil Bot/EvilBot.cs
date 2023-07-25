using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            return FindBestMove(board, 3, board.IsWhiteToMove);
        }

        public int materialEvaluation(Board board)
        {
            int evaluation = 0;

            // Loop through each piece type and color to evaluate their positions
            foreach (PieceType pieceType in Enum.GetValues(typeof(PieceType)))
            {
                for (int color = 0; color < 2; color++) // 0 for white, 1 for black
                {
                    ulong bitboard = board.GetPieceBitboard(pieceType, color == 0); // Get the bitboard for the piece and color

                    int count = 0;
                    while (bitboard != 0)
                    {
                        bitboard &= bitboard - 1;
                        count++;
                    }
                    evaluation += count * (color == 0 ? pieceValues[(int)pieceType] : -pieceValues[(int)pieceType]);
                }
            }

            // Return the final evaluation score
            return evaluation;
        }

        // Recursive Minimax function with Alpha-Beta Pruning
        public int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0 || board.IsInCheckmate() || board.IsDraw())
            {
                return materialEvaluation(board);
            }

            // Generate and loop through all possible moves for the current player
            foreach (Move move in board.GetLegalMoves())
            {
                // Apply the move to the current board (make sure to undo it later)
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

                // Undo the move
                board.UndoMove(move);

                // Alpha-Beta Pruning
                if (alpha >= beta)
                    break;
            }

            return maximizingPlayer ? alpha : beta;
        }

        public Move FindBestMove(Board board, int depth, bool isWhite)
        {
            int bestValue = isWhite ? int.MinValue : int.MaxValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            Move bestMove = Move.NullMove;

            // Generate and loop through all possible moves for the current player
            foreach (Move move in board.GetLegalMoves())
            {
                // Apply the move to the current board (make sure to undo it later)
                board.MakeMove(move);

                int value = Minimax(board, depth - 1, alpha, beta, !isWhite);

                // Undo the move
                board.UndoMove(move);

                if ((isWhite && value > bestValue) || (!isWhite && value < bestValue))
                {
                    bestValue = value;
                    bestMove = move;
                }

                // Update alpha for white, beta for black
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


    }
}