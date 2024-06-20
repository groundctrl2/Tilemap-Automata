using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Cell
{
    public Vector3Int position;
    public CellState state;

    public Cell(Vector3Int newPosition, CellState newState)
    {
        position = newPosition;
        state = newState;
    }

    // Cells only equal each other if same position
    public override bool Equals(object obj)
    {
        if (obj is Cell other)
        {
            return position.Equals(other.position);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }
}
