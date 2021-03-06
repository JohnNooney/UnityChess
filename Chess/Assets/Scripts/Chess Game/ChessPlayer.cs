﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ChessPlayer 
{
   public TeamColor team { get; set; }
    public Board board { get; set;  }
    public List<Piece> activePieces { get; private set; }
    public List<Piece> oldActivePieces { get; private set; }

    public List<Piece> quickActivePieces { get; private set; }

    public Stack<List<Piece>> activePieceMove;

    public int score;

    public ChessPlayer(TeamColor team, Board board)
    {
        this.board = board;
        this.team = team;
        activePieces = new List<Piece>();
        oldActivePieces = new List<Piece>();
        quickActivePieces = new List<Piece>();
        activePieceMove = new Stack<List<Piece>>();
    }

    public void AddPiece(Piece piece)
    {
        if (!activePieces.Contains(piece))
        {
            activePieces.Add(piece);
        }
    }

    public void RemovePiece(Piece piece)
    {
        if (activePieces.Contains(piece))
        {
            activePieces.Remove(piece);
        }
    }

    public void GenerateAllPossibleMoves()
    {
        foreach (var piece in activePieces)
        {
            if (board.HasPiece(piece))
            {
                piece.SelectAvaliableSquares();
            }
        }
    }

    public void SimulateGenerateAllPossibleMoves()
    {
        foreach (var piece in activePieces) //**available squares may not align with grid positions
        {
            if (board.HasPiece(piece))
            {
                piece.SelectAvaliableSquares(); 
            }
        }
    }

    public void SavePieceState()
    {
        if (oldActivePieces.Count != 0)
        {
            oldActivePieces.Clear();
        }
        
        oldActivePieces = activePieces.ToList();
    }

    public void QuickSavePieceState()
    {
        if (quickActivePieces.Count != 0)
        {
            quickActivePieces.Clear();
        }

        //quickActivePieces.AddRange(activePieces);
        quickActivePieces = activePieces.ToList();
        activePieceMove.Push(activePieces);
    }

    public void QuickReturnPieceState()
    {
        //if (activePieces.Count != 0)
        //{
        //    activePieces.Clear();
        //}

        //activePieces.AddRange(quickActivePieces);

        activePieces = activePieceMove.Peek();
        activePieceMove.Pop();
    }

    public void ReturnPieceState()
    {
        if (activePieces.Count != 0)
        {
            activePieces.Clear();
        }

        activePieces = oldActivePieces.ToList();
    }

    public void ResetAllPossibleMoves()
    {
        foreach (var piece in oldActivePieces)
        {
            if (board.HasPiece(piece))
            {
                piece.SelectAvaliableSquares();
            }
        }
    }

    public Piece[] GetOpponentAttackingPieces<T>() where T :Piece
    {
        return activePieces.Where(p => p.IsAttackingPieceOfType<T>()).ToArray();
    }

    public Piece[] GetPiecesOfType<T>() where T : Piece
    {
        return activePieces.Where(p => p is T).ToArray();
    }

    //When king is in check prevent all other moves besides the one to uncheck it
    public void RemoveMovesEnablingAttackOnPiece<T>(ChessPlayer opponent, Piece selectedPiece) where T : Piece
    {
        List<Vector2Int> coordsToRemove = new List<Vector2Int>();
        foreach (var coords in selectedPiece.availableMoves)
        {
            Piece pieceOnSquare = board.GetPieceOnSquare(coords);
            board.UpdateBoardOnPieceMove(coords, selectedPiece.occupiedSquare, selectedPiece, null);
            opponent.GenerateAllPossibleMoves();
            if (opponent.CheckIfisAttackingPiece<T>())
            {
                coordsToRemove.Add(coords);
            }
            board.UpdateBoardOnPieceMove(selectedPiece.occupiedSquare, coords, selectedPiece, pieceOnSquare);
        }

        foreach (var coords in coordsToRemove)
        {
            selectedPiece.availableMoves.Remove(coords);
        }

    }

    public void OnGameRestarted()
    {
        activePieces.Clear();
    }

    private bool CheckIfisAttackingPiece<T>() where T : Piece
    {
        foreach (var piece in activePieces)
        {
            if (board.HasPiece(piece) && piece.IsAttackingPieceOfType<T>())
            {
                return true;
            }
        }

        return false;
    }

    public bool BlockCheckMate<T>(ChessPlayer opponent) where T : Piece
    {
        foreach (var piece in activePieces)
        {
            foreach (var coords in piece.availableMoves)
            {
                Piece pieceOnCoords = board.GetPieceOnSquare(coords); 
                board.UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null); //temporarily move to square to see if valid move
                opponent.GenerateAllPossibleMoves();
                if (!opponent.CheckIfisAttackingPiece<T>()) //if piece blocks check
                {
                    board.UpdateBoardOnPieceMove(piece.occupiedSquare, coords, piece, pieceOnCoords);
                    return true;
                }
                board.UpdateBoardOnPieceMove(piece.occupiedSquare, coords, piece, pieceOnCoords);
            }
        }
        return false;
    }
}
