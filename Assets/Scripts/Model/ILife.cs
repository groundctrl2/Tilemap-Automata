using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface ILife
{
    // Given a hashset containing the current cells, model returns a cells hashset of the next generation based on model's logic
    public HashSet<Cell> Step(HashSet<Cell> currentCells);
}
