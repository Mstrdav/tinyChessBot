using ChessChallenge.API;
using System;

public enum PlayerColor
{
    White = 1,
    Black = -1
}


public class MyBot : IChessBot
{

    int[] pieceValues = {0,100,300,300,500,900,10000};
    PlayerColor botColor;
    int depthMax = 4;
    int minEval = -10_000;
    int maxEval =  10_000;
    //Dictionary<ulong,int> evaluatedPositions = new Dictionary<ulong,int>();

    int Evaluation(Board board, PlayerColor myBotColor)
    {
        int evaluation = 0;
        if(board.IsDraw())
            return evaluation;
        
        PieceList[] allPieceLists = board.GetAllPieceLists();
        foreach(PieceList pieceList in allPieceLists)
        {
            int pieceNumber = pieceList.Count;
            PieceType pieceType = pieceList.TypeOfPieceInList;
            int piecePoints = pieceValues[(int)pieceType]*pieceNumber;
            if(pieceList.IsWhitePieceList)
                evaluation += piecePoints;
            else
                evaluation -= piecePoints;
        }
        return (int)myBotColor*evaluation;
    }

    T Minimax<T>(Board board, int depth,string player, PlayerColor myBotColor, int alpha, int beta) 
    {
        Move[] moves = board.GetLegalMoves();
        if(depth == depthMax)
            return (T) Convert.ChangeType(Evaluation(board,myBotColor),typeof(T));

        else if(player == "BOT")
        {
            int bestEvaluation = minEval;
            Move bestMove = new Move();
            foreach(Move move in moves)
            {
                board.MakeMove(move);
                int minimaxSon = Minimax<int>(board,depth+1,"ADV",myBotColor,alpha,beta);
                if(minimaxSon > bestEvaluation)
                {
                    bestEvaluation = minimaxSon;
                    bestMove = move;
                }
                alpha = Math.Max(alpha,bestEvaluation);
                board.UndoMove(move);
                if(beta<=alpha)
                    break;
            }
            if(depth == 0)
                return (T) Convert.ChangeType(bestMove,typeof(T));
            return (T) Convert.ChangeType(bestEvaluation,typeof(T));
        }

        else
        {
            int worstEvaluation = maxEval;
            foreach(Move move in moves)
            {
                board.MakeMove(move);
                int minimaxSon = Minimax<int>(board,depth+1,"BOT",myBotColor,alpha,beta);
                if(minimaxSon < worstEvaluation)
                    worstEvaluation = minimaxSon;
                beta = Math.Min(beta,worstEvaluation);
                board.UndoMove(move);
                if(beta <= alpha)
                    break;
            }
            return (T) Convert.ChangeType(worstEvaluation,typeof(T));
        }
    }
    public Move Think(Board board, Timer timer)
    {
        //adapter prof au temps restant et au precedent temps de calcul
        if(board.IsWhiteToMove)
            botColor = PlayerColor.White;
        else 
            botColor = PlayerColor.Black;
        return Minimax<Move>(board,0,"BOT",botColor,minEval,maxEval);
    }
}
