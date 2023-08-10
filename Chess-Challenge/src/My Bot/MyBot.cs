using ChessChallenge.API;
using System;

public enum PlayerColor
{
    White = 1,
    Black = -1
}


// public class MyBot : IChessBot
// {

//     int[] pieceValues = {0,100,300,300,500,900,140000};
//     PlayerColor botColor;
//     int depthMax = 4;
//     int minEval = -10_000;
//     int maxEval =  10_000;
//     //Dictionary<ulong,int> evaluatedPositions = new Dictionary<ulong,int>();

//     int Evaluation(Board board, PlayerColor myBotColor)
//     {
//         int evaluation = 0;
//         if(board.IsDraw())
//             return evaluation;

//         PieceList[] allPieceLists = board.GetAllPieceLists();
//         foreach(PieceList pieceList in allPieceLists)
//         {
//             int pieceNumber = pieceList.Count;
//             PieceType pieceType = pieceList.TypeOfPieceInList;
//             int piecePoints = pieceValues[(int)pieceType]*pieceNumber;
//             if(pieceList.IsWhitePieceList)
//                 evaluation += piecePoints;
//             else
//                 evaluation -= piecePoints;
//         }
//         return (int)myBotColor*evaluation;
//     }

//     T Minimax<T>(Board board, int depth,string player, PlayerColor myBotColor, int alpha, int beta) 
//     {
//         Move[] moves = board.GetLegalMoves();
//         if(depth == depthMax)
//             return (T) Convert.ChangeType(Evaluation(board,myBotColor),typeof(T));

//         else if(player == "BOT")
//         {
//             int bestEvaluation = minEval;
//             Move bestMove = new Move();
//             foreach(Move move in moves)
//             {
//                 board.MakeMove(move);
//                 int minimaxSon = Minimax<int>(board,depth+1,"ADV",myBotColor,alpha,beta);
//                 if(minimaxSon > bestEvaluation)
//                 {
//                     bestEvaluation = minimaxSon;
//                     bestMove = move;
//                 }
//                 alpha = Math.Max(alpha,bestEvaluation);
//                 board.UndoMove(move);
//                 if(beta<=alpha)
//                     break;
//             }
//             if(depth == 0)
//                 return (T) Convert.ChangeType(bestMove,typeof(T));
//             return (T) Convert.ChangeType(bestEvaluation,typeof(T));
//         }

//         else
//         {
//             int worstEvaluation = maxEval;
//             foreach(Move move in moves)
//             {
//                 board.MakeMove(move);
//                 int minimaxSon = Minimax<int>(board,depth+1,"BOT",myBotColor,alpha,beta);
//                 if(minimaxSon < worstEvaluation)
//                     worstEvaluation = minimaxSon;
//                 beta = Math.Min(beta,worstEvaluation);
//                 board.UndoMove(move);
//                 if(beta <= alpha)
//                     break;
//             }
//             return (T) Convert.ChangeType(worstEvaluation,typeof(T));
//         }
//     }
//     public Move Think(Board board, Timer timer)
//     {
//         //adapter prof au temps restant et au precedent temps de calcul
//         if(board.IsWhiteToMove)
//             botColor = PlayerColor.White;
//         else 
//             botColor = PlayerColor.Black;
//         return Minimax<Move>(board,0,"BOT",botColor,minEval,maxEval);
//     }
// }

public class MyBot : IChessBot
{
    PlayerColor botColor;
    int depthMax = 4;
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 140000 };

    //Dictionary<ulong,int> evaluatedPositions = new Dictionary<ulong,int>();

    int Evaluation(Board board, PlayerColor myBotColor)
    {
        int evaluation = 0;
        if (board.IsDraw())
            return evaluation;

        PieceList[] allPieceLists = board.GetAllPieceLists();
        foreach (PieceList pieceList in allPieceLists)
        {
            int pieceNumber = pieceList.Count;
            PieceType pieceType = pieceList.TypeOfPieceInList;
            int piecePoints = pieceValues[(int)pieceType] * pieceNumber;
            if (pieceList.IsWhitePieceList)
                evaluation += piecePoints;
            else
                evaluation -= piecePoints;
        }

        return (int)myBotColor * evaluation * (board.IsWhiteToMove ? -1 : 1);
    }

    (Move, int) Minimax(Board board, int depth, string player, PlayerColor myBotColor, int alpha, int beta)
    {
        Move[] moves = board.GetLegalMoves();
        // if no legal moves, check if checkmate or stalemate
        if (moves.Length == 0)
        {
            if (board.IsInCheck())
            {
                return (new Move(), player == "BOT" ? -14000 : 14000);
            }
            else
                return (new Move(), 0);
        }

        if (depth == 0)
            return (new Move(), Evaluation(board, myBotColor));

        else if (player == "BOT")
        {
            int bestEvaluation = -14000;
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
            int worstEvaluation = 14000;
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

        // change depthMax depending on how many moves are possible

        if (depthMax != (board.GetLegalMoves().Length < 12 ? 6 : 4) && !board.IsInCheck())
        {
            Console.WriteLine("Depth changed to " + (board.GetLegalMoves().Length < 12 ? 6 : 4) + " because there are " + board.GetLegalMoves().Length + " moves possible.");
            depthMax = board.GetLegalMoves().Length < 12 ? 6 : 4;
        }

        return Minimax(board, depthMax, "BOT", botColor, -14000, 14000).Item1;
    }
}