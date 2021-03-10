using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MaterialSetter))]
[RequireComponent(typeof(IObjectTweener))]
public abstract class Piece : MonoBehaviour
{
	[SerializeField] private MaterialSetter materialSetter;
	public Board board { protected get; set; }
	public Vector2Int occupiedSquare { get; set; }

	//store the coords to revert back to after a simulated move
	public Vector2Int oldOccupiedSquare { get; set; } 

	public TeamColor team { get; set; }
	public bool hasMoved { get; private set; }
	public List<Vector2Int> availableMoves;
	public List<Vector2Int> availableMovesCopy;

	private IObjectTweener tweener;

	public abstract List<Vector2Int> SelectAvaliableSquares();

	private void Awake()
	{
		availableMoves = new List<Vector2Int>();
		availableMovesCopy = new List<Vector2Int>();
		tweener = GetComponent<IObjectTweener>();
		materialSetter = GetComponent<MaterialSetter>();
		hasMoved = false;
	}

	public void CopyMoves()
    {
        if (availableMovesCopy.Count != 0)
        {
			availableMoves.Clear();
        }
		availableMovesCopy = availableMoves.ToList();
    }

	public void SetMaterial(Material selectedMaterial)
	{
		materialSetter.SetSingleMaterial(selectedMaterial);
	}

	public bool IsFromSameTeam(Piece piece)
	{
		return team == piece.team;
	}

	public bool CanMoveTo(Vector2Int coords)
	{
		return availableMoves.Contains(coords);
	}

	public virtual void MovePiece(Vector2Int coords)
	{
        if (board.moveSimulation == true)
        {
			occupiedSquare = coords;
		}
		else
        {
			Vector3 targetPosition = board.CalculatePosFromCoords(coords);
			occupiedSquare = coords;
			hasMoved = true;
			tweener.MoveTo(transform, targetPosition);
		}
		
	}

	public bool IsAttackingPieceOfType<T>() where T : Piece
    {
        foreach (var square in availableMoves)
        {
            if (board.GetPieceOnSquare(square) is T)
            {
				return true;
            }
        }
		return false;
    }

    protected void TryToAddMove(Vector2Int coords)
	{
		availableMoves.Add(coords);
	}

	public void SetData(Vector2Int coords, TeamColor team, Board board)
	{
		this.team = team;
		occupiedSquare = coords;
		this.board = board;
		transform.position = board.CalculatePosFromCoords(coords);
	}
}