using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PieceCreation))]
public class ChessGameControl : MonoBehaviour
{
    private enum GameState { Init, Play, Finished}

    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;
    [SerializeField] private ChessUIManager manager;

    private PieceCreation pieceCreator;
    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    private ChessPlayer activePlayer;

    private GameState currentState;

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
        manager.HideUI(); //hide game over ui
        SetGameState(GameState.Init);
        board.SetDependencies(this);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);
        SetGameState(GameState.Play);
    }

    public void RestartGame()
    {
        DestroyPieces();
        board.OnGameRestarted();
        whitePlayer.OnGameRestarted();
        blackPlayer.OnGameRestarted();
        StartNewGame();
    }

    private void DestroyPieces()
    {
        whitePlayer.activePieces.ForEach(p => Destroy(p.gameObject));
        blackPlayer.activePieces.ForEach(p => Destroy(p.gameObject));
    }

    private void SetGameState(GameState state)
    {
        this.currentState = state;
    }

    public bool IsGameInProgress()
    {
        return currentState == GameState.Play;
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

    public void CreatePieceAndInit(Vector2Int squareCoords, TeamColor team, Type type)
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

    public void EndTurn()
    {
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        if (CheckGameIsFinished())
        {
            EndGame();
        }
        else
        {
            ChangeActiveTeam();
        }
    }

    //Check if there is a check mate on the board
    private bool CheckGameIsFinished()
    {
        Piece[] kingAttackingPieces = activePlayer.GetOpponentAttackingPieces<King>();
        if (kingAttackingPieces.Length > 0)
        {
            ChessPlayer oppositePlayer = GetOpponentToPlayer(activePlayer);
            Piece attackedKing = oppositePlayer.GetPiecesOfType<King>().FirstOrDefault();
            oppositePlayer.RemoveMovesEnablingAttackOnPiece<King>(activePlayer, attackedKing);

            int availbaleKingMoves = attackedKing.availableMoves.Count;
            if (availbaleKingMoves == 0) //if king cant move, see if piece can block check
            {
                bool canCoverKing = oppositePlayer.BlockCheckMate<King>(activePlayer);
                if (!canCoverKing) 
                {
                    return true; //if no piece can block then game over
                }
            }
        }
        return false;
    }

    public void OnPieceRemoved(Piece piece)
    {
        ChessPlayer pieceOwner = (piece.team == TeamColor.White) ? whitePlayer : blackPlayer;
        pieceOwner.RemovePiece(piece);
        Destroy(piece.gameObject);
    }

    private void EndGame()
    {
        Debug.Log("Game Over");
        manager.OnGameFinished(activePlayer.team.ToString());
        SetGameState(GameState.Finished);
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

    /// <summary>
    /// when in check remove square selector from invalid moves
    /// </summary>
    public void NullifyInvalidAttacksOnType<T>(Piece piece) where T : Piece
    {
        activePlayer.RemoveMovesEnablingAttackOnPiece<T>(GetOpponentToPlayer(activePlayer), piece);
    }
}
