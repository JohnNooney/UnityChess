using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to generate pieces place on board creation
/// </summary>
[CreateAssetMenu(menuName ="Scriptable Objects/Board/Layout")]
public class BoardLayout : ScriptableObject
{
    /// <summary>
    /// defines the attributes of each piece created
    /// </summary>
    [Serializable]
    private class BoardSquareSetup
    {
        public Vector2Int position;
        public PieceType pieceType;
        public TeamColor teamColor;
    }

    [SerializeField] 
    private BoardSquareSetup[] boardSquares;

    public int GetPiecesCount()
    {
        return boardSquares.Length;
    }

    public Vector2Int GetSquareCoordAtIndex(int index)
    {
        //set default value if index not specificed on board layout
        if (boardSquares.Length <= index)
        {
            Debug.LogError("Index of piece is out of range ");
            return new Vector2Int(-1, -1);
        }
        return new Vector2Int(boardSquares[index].position.x - 1, boardSquares[index].position.y - 1);
    }

    public string GetSquarePieceNameAtIndex(int index)
    {
        if (boardSquares.Length <= index)
        {
            Debug.LogError("Index of piece is out of range ");
            return "";
        }

        return boardSquares[index].pieceType.ToString();
    }

    public TeamColor GetSquareTeamAtIndex(int index)
    {
        if (boardSquares.Length <= index)
        {
            Debug.LogError("Index of piece is out of range ");
            return TeamColor.Black;
        }
        return boardSquares[index].teamColor;
    }
}
