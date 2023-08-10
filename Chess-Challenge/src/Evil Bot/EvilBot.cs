using ChessChallenge.API;
using System;
using System.Collections;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        // // Piece values: null, pawn, knight, bishop, rook, queen, king
        // int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        // public Move Think(Board board, Timer timer)
        // {
        //     Move[] allMoves = board.GetLegalMoves();

        //     // Pick a random move to play if nothing better is found
        //     Random rng = new();
        //     Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        //     int highestValueCapture = 0;

        //     foreach (Move move in allMoves)
        //     {
        //         // Always play checkmate in one
        //         if (MoveIsCheckmate(board, move))
        //         {
        //             moveToPlay = move;
        //             break;
        //         }

        //         // Find highest value capture
        //         Piece capturedPiece = board.GetPiece(move.TargetSquare);
        //         int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

        //         if (capturedPieceValue > highestValueCapture)
        //         {
        //             moveToPlay = move;
        //             highestValueCapture = capturedPieceValue;
        //         }
        //     }

        //     return moveToPlay;
        // }

        // // Test if this move gives checkmate
        // bool MoveIsCheckmate(Board board, Move move)
        // {
        //     board.MakeMove(move);
        //     bool isMate = board.IsInCheckmate();
        //     board.UndoMove(move);
        //     return isMate;
        // }

        PlayerColor botColor;
        int depthMax = 4;
        // evaluate position is a has table referencing foreach position evaluated
        // its evaluation and the depth at which it was evaluated
        Hashtable evaluatedPositions = new Hashtable();

        int Evaluation(Board board, PlayerColor myBotColor, int depth)
        {
            // check in hashtable if position has already been evaluated with a depth >= current depth
            // if yes, return the evaluation
            // if no, evaluate the position and add it to the hashtable
            if (evaluatedPositions.ContainsKey(board.ZobristKey))
            {
                (int, int) evaluationAndDepth = ((int, int)) evaluatedPositions[board.ZobristKey];
                if (evaluationAndDepth.Item2 >= depth)
                    return evaluationAndDepth.Item1;
            }

            int evaluation = 0;
            if (board.IsDraw())
                return evaluation;

            // PieceList[] allPieceLists = board.GetAllPieceLists();
            // foreach (PieceList pieceList in allPieceLists)
            // {
            //     int pieceNumber = pieceList.Count;
            //     PieceType pieceType = pieceList.TypeOfPieceInList;
            //     int piecePoints = pieceValues[(int)pieceType] * pieceNumber;
            //     if (pieceList.IsWhitePieceList)
            //         evaluation += piecePoints;
            //     else
            //         evaluation -= piecePoints;
            // }

            evaluation = GetLegalMoves(board, "ADV").Length - GetLegalMoves(board, "BOT").Length;

            // add evaluation to hashtable
            evaluation = (int)myBotColor * evaluation * (board.IsWhiteToMove ? -1 : 1);

            evaluatedPositions.Add(board.ZobristKey, (evaluation, depth));
            return evaluation;
        }

        (Move, int) Minimax(Board board, int depth, string player, PlayerColor myBotColor, int alpha, int beta)
        {
            Move[] moves = board.GetLegalMoves();
            // if no legal moves, check if checkmate or stalemate
            if (moves.Length == 0)
            {
                if (board.IsInCheck())
                {
                    return (new Move(), player == "BOT" ? -1000 : 1000);
                }
                else
                    return (new Move(), 0);
            }

            if (depth == 0)
                return (new Move(), Evaluation(board, myBotColor, depth));

            else if (player == "BOT")
            {
                int bestEvaluation = -1000;
                Move bestMove = moves[0];
                foreach (Move move in moves)
                {
                    board.MakeMove(move);
                    (Move, int) minimaxSon = Minimax(board, depth - 1, "ADV", myBotColor, alpha, beta);
                    if (minimaxSon.Item2 > bestEvaluation)
                    {
                        bestEvaluation = minimaxSon.Item2;
                        bestMove = move;
                    }
                    alpha = Math.Max(alpha, bestEvaluation);
                    board.UndoMove(move);
                    if (beta <= alpha)
                        break;
                }
                return (bestMove, bestEvaluation);
            }

            else
            {
                int worstEvaluation = 1000;
                foreach (Move move in moves)
                {
                    board.MakeMove(move);
                    (Move, int) minimaxSon = Minimax(board, depth - 1, "BOT", myBotColor, alpha, beta);
                    if (minimaxSon.Item2 < worstEvaluation)
                        worstEvaluation = minimaxSon.Item2;
                    beta = Math.Min(beta, worstEvaluation);
                    board.UndoMove(move);
                    if (beta <= alpha)
                        break;
                }
                return (new Move(), worstEvaluation);
            }
        }
        public Move Think(Board board, Timer timer)
        {
            botColor = board.IsWhiteToMove ? PlayerColor.White : PlayerColor.Black;
            return Minimax(board, depthMax, "BOT", botColor, -1000, 1000).Item1;
        }

        // public Move Think(Board board, Timer timer)
        // {
        //     // The lawyer bot
        //     // attemps to maximize the number of legal moves, and minimizes the number of legal moves of the opponent

        //     PlayerColor myBotColor = board.IsWhiteToMove ? PlayerColor.White : PlayerColor.Black;
        //     Console.WriteLine("My color is " + myBotColor);

        //     int depth = 2; // for now

        //     (Move move, int evaluation) = SimpleMinimax(board, depth, "BOT", myBotColor, alpha: -1000, beta: 1000);
        //     Console.WriteLine("Best move is " + move + " with evaluation " + evaluation);
        //     return move;
        // }

        public Move[] GetLegalMoves(Board board, string player)
        {
            // check is the requested player is playing
            if (player == "BOT")
                return board.GetLegalMoves();
            else
            {
                board.ForceSkipTurn();
                Move[] moves = board.GetLegalMoves();
                board.UndoSkipTurn();
                return moves;
            }
        }

        // public (Move, int) SimpleMinimax(Board board, int depth, string player, PlayerColor myBotColor, int alpha, int beta)
        // {
        //     Move[] moves = GetLegalMoves(board, player, myBotColor);
        //     // if no legal moves, check if checkmate or stalemate
        //     if (moves.Length == 0)
        //     {
        //         if (board.IsInCheck())
        //         {
        //             return (new Move(), player == "BOT" ? -1000 : 1000);
        //         }
        //         else
        //             return (new Move(), 0);
        //     }

        //     // if (depth == 0)
        //     //     return (new Move(), GetLegalMoves(board, player, myBotColor).Length - GetLegalMoves(board, player == "BOT" ? "ADV" : "BOT", myBotColor).Length);

        //     // else if (player == "BOT")
        //     // {
        //     //     int bestEvaluation = -1000;
        //     //     Move bestMove = moves[0];
        //     //     foreach (Move move in moves)
        //     //     {
        //     //         board.MakeMove(move);
        //     //         (Move, int) minimaxSon = SimpleMinimax(board, depth - 1, "ADV", myBotColor);
        //     //         if (minimaxSon.Item2 > bestEvaluation)
        //     //         {
        //     //             bestEvaluation = minimaxSon.Item2;
        //     //             bestMove = move;
        //     //         }
        //     //         board.UndoMove(move);
        //     //     }
        //     //     return (bestMove, bestEvaluation);
        //     // }

        //     // else
        //     // {
        //     //     int worstEvaluation = 1000;
        //     //     Move bestMove = moves[0];
        //     //     foreach (Move move in moves)
        //     //     {
        //     //         board.MakeMove(move);
        //     //         (Move, int) minimaxSon = SimpleMinimax(board, depth - 1, "BOT", myBotColor);
        //     //         if (minimaxSon.Item2 < worstEvaluation)
        //     //         {
        //     //             worstEvaluation = minimaxSon.Item2;
        //     //             bestMove = move;
        //     //         }
        //     //         Console.WriteLine("Move " + move + " has evaluation " + minimaxSon.Item2);
        //     //         Console.WriteLine(board);
        //     //         board.UndoMove(move);
        //     //     }
        //     //     return (bestMove, worstEvaluation);
        //     // }

        //     if (depth == 0)
        //         //     return (new Move(), GetLegalMoves(board, player, myBotColor).Length - GetLegalMoves(board, player == "BOT" ? "ADV" : "BOT", myBotColor).Length);
        //         return (new Move(), 0);

        //     else if (player == "BOT")
        //     {
        //         int bestEvaluation = -1000;
        //         Move bestMove = moves[0];
        //         foreach (Move move in moves)
        //         {
        //             board.MakeMove(move);
        //             (Move,int) minimaxSon = SimpleMinimax(board, depth - 1, "ADV", myBotColor, alpha, beta);
        //             if (minimaxSon.Item2 > bestEvaluation)
        //             {
        //                 bestEvaluation = minimaxSon.Item2;
        //                 bestMove = move;
        //             }
        //             alpha = Math.Max(alpha, bestEvaluation);
        //             board.UndoMove(move);
        //             if (beta <= alpha)
        //                 break;
        //         }
        //         return (bestMove, bestEvaluation);
        //     }

        //     else
        //     {
        //         int worstEvaluation = 1000;
        //         foreach (Move move in moves)
        //         {
        //             board.MakeMove(move);
        //             (Move, int) minimaxSon = SimpleMinimax(board, depth - 1, "BOT", myBotColor, alpha, beta);
        //             if (minimaxSon.Item2 < worstEvaluation)
        //                 worstEvaluation = minimaxSon.Item2;
        //             beta = Math.Min(beta, worstEvaluation);
        //             board.UndoMove(move);
        //             if (beta <= alpha)
        //                 break;
        //         }
        //         return (new Move(), worstEvaluation);
        //     }
        // }
    }
}