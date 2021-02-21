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
    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    private ChessPlayer activePlayer;

    private void Awake()
    {
        setDependencies();
        CreatePlayers();
    }

    private void setDependencies()
    {
        pieceCreator = GetComponent<PieceCreation>();
    }

    private void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }

    void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        board.SetDependencies(this);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);
    }

    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        //for each piece initialized get 
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordAtIndex(i);
            TeamColor team = layout.GetSquareTeamAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);

            Debug.Log(team + " " + typeName + " at " + squareCoords.x + "," + squareCoords.y);

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

        board.setPieceOnBoard(squareCoords, newPiece);

        ChessPlayer currentPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(newPiece);
    }

    //checks if piece selected is from same team or not
    public bool IsTeamTurnActive(TeamColor team)
    {
        return activePlayer.team == team;
    }

    private void GenerateAllPossiblePlayerMoves(ChessPlayer player)
    {
        player.GenerateAllPossibleMoves();
    }

    internal void EndTurn()
    {
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        ChangeActiveTeam();
    }

    private void ChangeActiveTeam()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player)
    {
        //switch teams (if white go to black and vice versa)
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }
}
