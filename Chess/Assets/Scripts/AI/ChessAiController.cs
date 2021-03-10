using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChessAiController
{
    private double maxEval = 0;
    private double minEval = 0;
    private double currentEval = 0;
    private Piece bestPiece;
    private Vector2Int bestMove;
    private List<Piece> activePiecesCopy = new List<Piece>();
    private List<Vector2Int> availableMovesCopy = new List<Vector2Int>();

    private ChessGameControl chessController; //facade

    public void SetDependencies(ChessGameControl chessController)
    {
        this.chessController = chessController;
    }

    //computer's random algorithm for playing the game
    public double MiniMax(Piece pieceToMove,Vector2Int position, int depth, bool maximizing_player, Board board)
    {
        //Piece piece = board.GetPieceOnSquare(position);
        if (pieceToMove != null)
        {
            board.SimulateMove(pieceToMove, position);
        }

        //base case: check that depth is already reached or gameover
        if (depth == 0 || chessController.CheckGameIsFinished())
        {
            Debug.Log("Max Depth Reached. Ending Team: "+ chessController.computer.team + " with " + pieceToMove.name);
            return Evaluate(pieceToMove);
        }

        //check to see if maximizing player or min
        if (maximizing_player)
        {
            chessController.computer = chessController.blackPlayer;
            maxEval = double.NegativeInfinity;

            activePiecesCopy = chessController.computer.activePieces.ToList();
            foreach (var newPiece in activePiecesCopy)
            {
                
                newPiece.CopyMoves();
                if (newPiece.availableMovesCopy.Count != 0)
                {
                    Debug.Log("Black Turn: " + newPiece.name + " " + newPiece.team + " at " + newPiece.occupiedSquare.x + "," + newPiece.occupiedSquare.y);

                    foreach (var move in newPiece.availableMovesCopy)
                    {

                        //save the current grid state to revert back to for each move
                        board.QuickSaveState();
                        //save the occupied square of piece before moving it
                        newPiece.oldOccupiedSquare = newPiece.occupiedSquare;



                        Debug.Log(chessController.computer.team + " tried to move: " + newPiece.name + " to " + move.x + "," + move.y);

                        //simulate the board move
                        //board.SimulateMove(newPiece, move);
                        //board.UpdateBoardOnPieceMove(move, newPiece.occupiedSquare, newPiece, null);
                        currentEval = MiniMax(newPiece, move, depth - 1, false, board);

                        if (Evaluate(newPiece) >= maxEval)
                        {
                            maxEval = currentEval;
                            SetBestResult(newPiece, move);
                        }

                        //revert board back to starting state
                        board.QuickReturnState();
                        newPiece.occupiedSquare = newPiece.oldOccupiedSquare;
                        //chessController.SimulateEndTurn();
                    }
                }
            }

            //chessController.computer = chessController.whitePlayer;
            return maxEval;
            
        }
        else //simulate human move
        {
            chessController.computer = chessController.whitePlayer;
            minEval = double.PositiveInfinity;

            foreach (var oppositePiece in chessController.computer.activePieces)
            {
                if (oppositePiece.availableMoves.Count != 0)
                {
                    foreach (var move in oppositePiece.availableMoves)
                    {
                        board.QuickSaveState();

                        //select other teams piece piece (currently still selected knight)
                        if (chessController.computer == chessController.whitePlayer)
                        {
                            Debug.Log("Whites Turn: " + oppositePiece.name + " " + oppositePiece.team + " at " + oppositePiece.occupiedSquare.x + "," + oppositePiece.occupiedSquare.y);
                        }

                        //board.SimulateMove(oppositePiece, move);
                        //board.UpdateBoardOnPieceMove(move, oppositePiece.occupiedSquare, oppositePiece, null);
                        Debug.Log(chessController.computer.team + " tried to move: " + oppositePiece.name + " to " + move.x + "," + move.y);
                        currentEval = MiniMax(oppositePiece, move, depth - 1, true, board);
                        if (Evaluate(oppositePiece) <= minEval)
                        {
                            minEval = currentEval;
                        }

                        board.QuickReturnState();
                    }
                }
            }

            //chessController.computer = chessController.blackPlayer;
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
