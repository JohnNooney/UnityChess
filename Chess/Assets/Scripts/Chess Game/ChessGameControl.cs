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

    private ChessAiController chessAi; //manages the ai
    private PieceCreation pieceCreator;
    public ChessPlayer whitePlayer;
    public ChessPlayer blackPlayer;
    public ChessPlayer activePlayer;

    //Personal Code: computer team identifier
    public ChessPlayer computer; //one of the players will be human, the other Computer
    public bool simulatedEndGame;

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

        chessAi = new ChessAiController();
        chessAi.SetDependencies(this);

        //calculate starting scores (should be 1290 for each team)
        board.GetBoardScore();
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

    private void SimulatedGenerateAllPossiblePlayerMoves(ChessPlayer player)
    {
        player.SimulateGenerateAllPossibleMoves();
    }

    public void ResetAllPossiblePlayerMoves(ChessPlayer player)
    {
        player.ResetAllPossibleMoves();
    }

    public void EndTurn()
    {
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));

        //before the end of the game check the scores of each player
        board.GetBoardScore();

        if (CheckGameIsFinished())
        {
            manager.onNewMove();
            EndGame();
        }
        else
        {
            manager.onNewMove();
            ChangeActiveTeam();
        }
    }

    //altered version of EndTurn() that doesn't change active teams
    public void SimulateEndTurn()
    {
        SimulatedGenerateAllPossiblePlayerMoves(computer);
        SimulatedGenerateAllPossiblePlayerMoves(GetOpponentToPlayer(computer));

        //before the end of the game check the scores of each player
        board.GetBoardScore();

        if (CheckGameIsFinished())
        {
            // manager.onNewMove();
            simulatedEndGame = true;
        }
        else
        {
            //manager.onNewMove();
            //computer = computer == whitePlayer ? blackPlayer : whitePlayer;
        }
    }

    //Check if there is a check mate on the board
    public bool CheckGameIsFinished()
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
        if (computer == whitePlayer || activePlayer == computer)
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
    public void RandComputerMove()
    {
        Thread.Sleep(200);
        while (true && !CheckGameIsFinished())
        {

            //randomly generate coordinates for piece selection
            int randActivePiece = UnityEngine.Random.Range(0, computer.activePieces.Count);
            Piece randPieceSelect = computer.activePieces[randActivePiece];
            board.ComputerInput(randPieceSelect.occupiedSquare.x, randPieceSelect.occupiedSquare.y); //select

            //if there is a selected piece
            if (board.selectedPiece != null && board.HasPiece(randPieceSelect) && randPieceSelect.availableMoves.Count > 0)
            {
                //pick a random element in the available moves list
                int randMove = UnityEngine.Random.Range(0, randPieceSelect.availableMoves.Count);
                Vector2Int randMoveCoords = randPieceSelect.availableMoves[randMove];

                board.ComputerInput(randMoveCoords.x, randMoveCoords.y); //move
                break;
            }
        }

    }

    //Make computer move for MiniMax Algorithm
    public void ComputerMove()
    {

        int depth = 1;
        double bestMax = 0;
        Tuple<Piece, Vector2Int> bestPieceAndMove = null;

        board.saveCurrentGridState();

        //iterate through every piece
        //foreach (var piece in computer.activePieces)
        //{
            //Vector2Int piecePosition = new Vector2Int(0, 0);

            //send piece coords, starting depth, and maximizing player start
            double currentBest = chessAi.MiniMax(null, new Vector2Int(0, 0), depth, true, board);
            if (currentBest >= bestMax)
            {
                bestMax = currentBest;
                bestPieceAndMove = chessAi.GetBestResult();
                Debug.Log("Best piece to move: " + bestPieceAndMove.Item1.name + " to " + bestPieceAndMove.Item2.x + "," + bestPieceAndMove.Item2.y);
            }
            
           
       // }

        board.returnToStartState();

        if (bestMax == 0)
        {
            Debug.Log("Random move hit.");
            RandComputerMove();
        }

        else if (bestPieceAndMove != null)
        {
            //select piece
            board.ComputerInput(bestPieceAndMove.Item1.occupiedSquare.x, bestPieceAndMove.Item1.occupiedSquare.y);
            //move piece
            board.ComputerInput(bestPieceAndMove.Item2.x, bestPieceAndMove.Item2.y);
        }
        
        else
        {
            Debug.Log("No pieces found. BestMax = " + bestMax);
        }
       

    }


}
