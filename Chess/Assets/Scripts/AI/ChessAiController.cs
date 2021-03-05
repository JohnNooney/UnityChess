using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessAiController
{
    private double maxEval = 0;
    private double minEval = 0;
    private double currentEval = 0;
    private Piece bestPiece;
    private Vector2Int bestMove;

    private ChessGameControl chessController; //facade

    public void SetDependencies(ChessGameControl chessController)
    {
        this.chessController = chessController;
    }

    //computer's random algorithm for playing the game
    public double MiniMax(Vector2Int position, int depth, bool maximizing_player, Board board)
    {
        Piece piece = board.GetPieceOnSquare(position);

        //base case: check that depth is already reached or gameover
        if (depth == 0 || chessController.CheckGameIsFinished())
        {
            Debug.Log("Max Depth Reached");
            return Evaluate(piece);
        }

        //check to see if maximizing player or min
        if (maximizing_player)
        {
            maxEval = double.NegativeInfinity;

                foreach (var move in piece.availableMoves)
                {
                    
                    //save the current grid state to revert back to
                    //board.saveCurrentGridState();

                    Debug.Log("tried to move: " + piece.name + " to " + move.x + "," + move.y);

                    //simulate the board move
                    //board.SimulateMove(piece, move);
                    currentEval = MiniMax(move, depth-1, false, board);

                    //check if game has ended with move
                    //if (chessController.simulatedEndGame)
                    //{
                    //    SetBestResult(piece, move);
                    //    return GetBestResult();
                    //}

                    if (Evaluate(piece) >= maxEval)
                    {
                        maxEval = currentEval;
                        SetBestResult(piece, move);
                    }

                    //recurse so that the next move up to the depth max is calculated
                    //MiniMax(depth - 1, false, board);

                    //choose to save move if better than last move checked
                    //if (Evaluate(chessController.computer) >= maxEval)
                    //{
                    //    SetBestResult(piece, move);
                    //}

                    //revert board back to starting state
                    //board.returnToStartState();
                }
            return maxEval;
            
        }
        else
        {
            minEval = double.PositiveInfinity;
            foreach (var move in piece.availableMoves)
            {
                board.SimulateMove(piece, move);
                currentEval = MiniMax(move, depth -1, true, board);
                if (Evaluate(piece) <= minEval)
                {
                    minEval = currentEval;
                }

            }
            return minEval;
        }

    }

    private void SetBestResult(Piece selectedPiece, Vector2Int selectedMove)
    {
        bestPiece = selectedPiece;
        bestMove = selectedMove;
    }

    public Tuple<Piece, Vector2Int> GetBestResult()
    {
        return Tuple.Create(bestPiece, bestMove);
    }

    //Personal Code: Heuristic Evaluation Function for MiniMax
    //upon each piece loss evaluate score compared to opponent
    private int Evaluate(Piece piece)
    {
        //check if the current piece evalutaed belongs to white or black
        if (piece != null && piece.team == chessController.whitePlayer.team)
        {
            return chessController.whitePlayer.score - chessController.blackPlayer.score;
        }
        else
        {
            return chessController.blackPlayer.score - chessController.whitePlayer.score;
        }
    }

}
