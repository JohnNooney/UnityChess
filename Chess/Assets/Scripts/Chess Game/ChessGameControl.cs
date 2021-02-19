using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PieceCreation))]
public class ChessGameControl : MonoBehaviour
{
    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;

    private PieceCreation pieceCreator;

    private void Awake()
    {
        setDependencies();
    }

    private void setDependencies()
    {
        pieceCreator = GetComponent<PieceCreation>();
    }

    void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        CreatePiecesFromLayout(startingBoardLayout);
    }

    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        //for each piece initialized get 
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordAtIndex(i);
            TeamColor team = layout.GetSquareTeamAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);

            Type type = Type.GetType(typeName);
            CreatePieceAndInit(squareCoords, team, type);
        }
    }

    private void CreatePieceAndInit(Vector2Int squareCoords, TeamColor team, Type type)
    {
        //choose the cooresponding prefab of the piece to the name given
        Piece newPiece = pieceCreator.CreatePiece(type).GetComponent<Piece>();
        newPiece.SetData(squareCoords, team, board);

        Material teamMaterial = pieceCreator.getTeamMaterial(team);
        newPiece.SetMaterial(teamMaterial);
    }
}
