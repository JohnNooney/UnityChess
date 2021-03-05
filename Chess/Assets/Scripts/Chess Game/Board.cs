using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SquareSelectorCreator))]
public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 8;

    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;

    private Piece[,] grid; //store where the pieces are placed
    private Piece[,] oldGrid; //store where the piece of the last move are on the grid
    private ChessGameControl chessController; //facade
    private SquareSelectorCreator squareSelector;

    //Personal Code: added score tracking into the game
    public Dictionary<String, int> pieceToValueDict = new Dictionary<String, int>();
    public int oldBlackPlayerScore;
    public int oldWhitePlayerScore;

    public Piece selectedPiece;
    public bool moveSimulation = false;


    private void Awake()
    {
        squareSelector = GetComponent<SquareSelectorCreator>();
        CreateGrid();
    }

    public void SetDependencies(ChessGameControl chessController)
    {
        this.chessController = chessController;

        //Personal Code: Init each piece's score weightss
        pieceToValueDict.Add("King", 900);
        pieceToValueDict.Add("Queen", 90);
        pieceToValueDict.Add("Rook", 50);
        pieceToValueDict.Add("Bishop", 30);
        pieceToValueDict.Add("Knight", 30);
        pieceToValueDict.Add("Pawn", 10);
    }

    private void CreateGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

    internal Vector3 CalculatePosFromCoords(Vector2Int coords)
    {
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize, 0f, coords.y * squareSize);
    }

    private Vector2Int CalculateCoordsFromPos(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).x / squareSize) + BOARD_SIZE /2;
        int y = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).z / squareSize) + BOARD_SIZE /2;

        //Debug.Log("Calculate click coords: " + x + "," + y);

        return new Vector2Int(x, y);
    }

    public void OnGameRestarted()
    {
        selectedPiece = null;
        CreateGrid();
    }

    public void OnSquareSelected(Vector3 inputPosition)
    {
        if (!chessController.IsGameInProgress()) //make sure game is in progress to allow moves
        {
            return;
        }
        Vector2Int coords = CalculateCoordsFromPos(inputPosition);
        Piece piece = GetPieceOnSquare(coords); //based on user click return piece location
        //check if piece has already been selected
        if (selectedPiece)
        {
            //Debug.Log("Selected Piece: " + selectedPiece.name + selectedPiece.team + " at " + coords.x + "," + coords.y); ;
            //check if selected piece is same as piece just clicked on
            if (piece != null && selectedPiece == piece)
            {
                DeselectPiece();
            }
            //check if selected piece is not the same as jsut clicked on and from the same team as active player
            else if(piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team))
            {
                SelectPiece(piece);
            }
            else if (selectedPiece.CanMoveTo(coords))
            {
                OnSelectedPieceMove(coords, selectedPiece);
            }
        }
        else //if no piece has been selected yet
        {
            if (piece != null && chessController.IsTeamTurnActive(piece.team))
            {
                SelectPiece(piece);
            }
        }
    }

    public void PromotePiece(Piece piece)
    {
        TakePiece(piece);
        chessController.CreatePieceAndInit(piece.occupiedSquare, piece.team, typeof(Queen));
    }

    public void setPieceOnBoard(Vector2Int coords, Piece piece)
    {
        if (CheckIfCoordAreOnBoard(coords))
        {
            //Debug.Log(piece.team + piece.name + " set at " + coords.x + "," + coords.y);
            grid[coords.x, coords.y] = piece;
        }
    }

    private void OnSelectedPieceMove(Vector2Int coords, Piece piece)
    {
        TryToTakeOppositePiece(coords);
        //update data on grid array (new coordinates, old coordinates, piece, piece in old coords)
        UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
        selectedPiece.MovePiece(coords);
        DeselectPiece();
        EndTurn();
    }

    private void TryToTakeOppositePiece(Vector2Int coords)
    {
        Piece piece = GetPieceOnSquare(coords);
        if (piece != null && !selectedPiece.IsFromSameTeam(piece))
        {
            if (moveSimulation)
            {
                SimulateTakePiece(piece);
            }
            TakePiece(piece);
        }
    }

    private void TakePiece(Piece piece)
    {
        if (piece)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            chessController.OnPieceRemoved(piece);
        }
    }

    //modified version of TakePiece which doesn't destory the physical piece only the grid position
    private void SimulateTakePiece(Piece piece)
    {
        if (piece)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            //chessController.OnPieceRemoved(piece);
        }
    }

    private void EndTurn()
    {
        chessController.EndTurn();
    }

    public void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece)
    {
        //Debug.Log("Old Coords: " + oldCoords.x + "," + oldCoords.y);
        //Debug.Log("New Coords: " + newCoords.x + "," + newCoords.y);
       // oldGrid = grid;
        grid[oldCoords.x, oldCoords.y] = oldPiece; 
        grid[newCoords.x, newCoords.y] = newPiece;
    }

    public void Undo()
    {
        grid = oldGrid; //remap old grid
        oldGrid = null; //clear the old move
    }

    private void SelectPiece(Piece piece)
    {
        chessController.NullifyInvalidAttacksOnType<King>(piece);
        selectedPiece = piece;
        List<Vector2Int> selection = selectedPiece.availableMoves;
        ShowSelectionSquares(selection);
    }

    private void ShowSelectionSquares(List<Vector2Int> selection)
    {
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
        for (int i = 0; i < selection.Count; i++)
        {
            Vector3 position = CalculatePosFromCoords(selection[i]);
            bool isSquareFree = GetPieceOnSquare(selection[i]) == null;
            squaresData.Add(position, isSquareFree);
        }
        squareSelector.ShowSelection(squaresData);
    }

    private void DeselectPiece()
    {
        selectedPiece = null;
        squareSelector.ClearSelection();
    }

    public Piece GetPieceOnSquare(Vector2Int coords)
    {
        if (CheckIfCoordAreOnBoard(coords))
        {
            return grid[coords.x, coords.y];
        }
        return null;
    }

    public bool CheckIfCoordAreOnBoard(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0 || coords.x >= BOARD_SIZE || coords.y >= BOARD_SIZE)
        {
            return false;
        }
        return true;
    }

    public bool HasPiece(Piece piece)
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                //if chess piece is already in a desired position
                if (grid[i,j] == piece)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //computer's random algorithm for playing the game
    public void ComputerInput(int xCoord, int yCoord)
    {
        Vector2Int coords = new Vector2Int(xCoord, yCoord);
        Piece piece = GetPieceOnSquare(coords);
        //check if piece has already been selected
        if (selectedPiece)
        {
            //Debug.Log("Selected Piece: " + selectedPiece.name + selectedPiece.team + " at " + coords.x + "," + coords.y); ;
            //check if selected piece is same as piece just clicked on
            if (piece != null && selectedPiece == piece)
            {
                DeselectPiece();
            }
            //check if selected piece is not the same as jsut clicked on and from the same team as active player
            else if (piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team))
            {
                SelectPiece(piece);
            }
            else if (selectedPiece.CanMoveTo(coords))
            {
                //attempt to take piece if that is desired action, otherwise just move
                OnSelectedPieceMove(coords, selectedPiece); 
                //flag = false;
            }
        }
        else //if no piece has been selected yet
        {
            if (piece != null && chessController.IsTeamTurnActive(piece.team))
            {
                SelectPiece(piece);
            }
        }
    }

    public void SimulateMove(Piece piece, Vector2Int move_coords)
    {
        moveSimulation = true; //start simulation

        if (piece != null || move_coords != null)
        {
            selectedPiece = piece;

            if (selectedPiece.CanMoveTo(move_coords))
            {
                //Move piece on grid (modified from OnSelectedPieceMove())
                TryToTakeOppositePiece(move_coords);
                //update data on grid array (new coordinates, old coordinates, piece, piece in old coords)
                UpdateBoardOnPieceMove(move_coords, piece.occupiedSquare, piece, null);
                //selectedPiece.MovePiece(move_coords); //officially moves piece (team score is updated)
                DeselectPiece();
                chessController.SimulateEndTurn();
            }
        }

        moveSimulation = false; //end simulation
    }

    public void saveCurrentGridState()
    {
        oldGrid = (Piece[,])grid.Clone(); //save piece coordinates
        oldBlackPlayerScore = chessController.blackPlayer.score; //save team scores
        oldWhitePlayerScore = chessController.whitePlayer.score;

    }

    public void returnToStartState()
    {
        //grid = oldGrid; //return to original piece coordinates
        Array.Copy(oldGrid, 0, grid, 0, oldGrid.Length);
        chessController.blackPlayer.score = oldBlackPlayerScore; //return to original team score
        chessController.whitePlayer.score = oldWhitePlayerScore;
    }
    public void GetBoardScore()
    {
        chessController.whitePlayer.score = 0;
        chessController.blackPlayer.score = 0;

        //for each place on the grid check if piece is there and based on which team it belongs
        //to add it to the score
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i,j] != null)
                {
                    int value; //var used to get value from dictionary
                    if (grid[i,j].team == chessController.whitePlayer.team)
                    {
                        if (pieceToValueDict.TryGetValue(grid[i, j].GetType().ToString(), out value))
                        {
                            chessController.whitePlayer.score += value;
                        }
                    }
                    else if (grid[i, j].team == chessController.blackPlayer.team)
                    {
                        if (pieceToValueDict.TryGetValue(grid[i, j].GetType().ToString(), out value))
                        {
                            chessController.blackPlayer.score += value;
                        }
                    }
                    
                }
            }
        }


        Debug.Log(chessController.whitePlayer.team + " score is: " + chessController.whitePlayer.score + "\n"
            + chessController.blackPlayer.team + " score is: " + chessController.blackPlayer.score);
    }


}
