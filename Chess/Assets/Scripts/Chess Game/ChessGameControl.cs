using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    //Personal Code: added score tracking into the game
    public Dictionary<String, int> pieceToValueDict = new Dictionary<String, int>();
    //public int blackScore;
    //public int whiteScore;

    //Personal Code: computer team identifier
    private ChessPlayer computer; //one of the players will be human, the other Computer

    private GameState currentState;

    private void Awake()
    {
        setDependencies();
        CreatePlayers();
    }

    private void setDependencies()
    {
        pieceCreator = GetComponent<PieceCreation>();

        //Personal Code: Init each piece's score weightss
        pieceToValueDict.Add("King", 900);
        pieceToValueDict.Add("Queen", 90);
        pieceToValueDict.Add("Rook", 50);
        pieceToValueDict.Add("Bishop", 30);
        pieceToValueDict.Add("Knight", 30);
        pieceToValueDict.Add("Pawn", 10);
    }

    private void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);

        //set computer color
        computer = blackPlayer;
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

        //calculate starting scores (should be 1290 for each team)
        GetBoardScore(whitePlayer);
        GetBoardScore(blackPlayer);
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

            //Debug.Log(team + " " + typeName + " at " + squareCoords.x + "," + squareCoords.y);

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

        //before the end of the game check the scores of each player
        GetBoardScore(activePlayer);

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

        //check if the active player is the computer
        if (ComputerMoveCheck())
        {
            ComputerMove();
        }
    }

    public bool ComputerMoveCheck()
    {
        if (activePlayer == computer)
        {
            return true;
        }
        return false;
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


    //Personal Code: Currently randomly selects an active piece on the board
    //from that piece, randomly select a possible move
    public void ComputerMove()
    {
        Thread.Sleep(200);
        while (true && !CheckGameIsFinished())
        {

            //randomly generate coordinates for piece selection
            int randActivePiece = UnityEngine.Random.Range(0, computer.activePieces.Count);
            Piece randPieceSelect = computer.activePieces[randActivePiece];
            board.ComputerInputRandom(randPieceSelect.occupiedSquare.x, randPieceSelect.occupiedSquare.y); //select

            //if there is a selected piece
            if (board.selectedPiece != null && board.HasPiece(randPieceSelect) && randPieceSelect.availableMoves.Count > 0)
            {
                //pick a random element in the available moves list
                int randMove = UnityEngine.Random.Range(0, randPieceSelect.availableMoves.Count);
                Vector2Int randMoveCoords = randPieceSelect.availableMoves[randMove];

                board.ComputerInputRandom(randMoveCoords.x, randMoveCoords.y); //move
                break;
            }
        }

    }

    private void GetBoardScore(ChessPlayer player)
    {
        //depending on which player's score is being tested reset it and count again
        player.score = 0;

        foreach (var activePiece in player.activePieces)
        {
            foreach (var pair in pieceToValueDict)
            {
                if (activePiece.GetComponent<Piece>().GetType().ToString() == pair.Key)
                {
                    player.score += pair.Value;
                }
            }
        }

        Debug.Log( player.team + " score is: " + player.score);
    }

    //Personal Code: Heuristic Evaluation Function for MiniMax
    //upon each piece loss evaluate score compared to opponent
    private int Evaluate(ChessPlayer player)
    {
        if (player == whitePlayer)
        {
            return whitePlayer.score - blackPlayer.score;
        }
        else
        {
            return blackPlayer.score - whitePlayer.score;
        }
    }
}
