using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockPaperScissorsLife : ILife
{
    private DrawGrid drawGrid;

    public RockPaperScissorsLife(DrawGrid grid)
    {
        drawGrid = grid;
    }

    public HashSet<Cell> Step(HashSet<Cell> currentCells)
    {
        HashSet<Cell> newCells = new HashSet<Cell>();
        HashSet<Cell> cellsToCheck = GetNeighbors(currentCells);

        foreach (Cell cell in cellsToCheck)
        {
            if (cell.state == CellState.stage1 || cell.state == CellState.stage2 || cell.state == CellState.stage3)
            {
                // TODO Game Logic
            }
            // else cell is pattern/user added of a CellState not recognized by model
            else
            {
                // TODO 
            }
        }

        return newCells;
    }

    // Builds and returns a hashset containing the current cells as well as their neighboring cells to be checked by game logic
    private HashSet<Cell> GetNeighbors(HashSet<Cell> currentCells)
    {
        HashSet<Cell> cellsToCheck = new HashSet<Cell>();

        if (currentCells.Count != 0 && currentCells != null)
        {
            foreach (Cell cell in currentCells)
            {
                if (cell != null)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            Vector3Int position = cell.position + new Vector3Int(x, y, 0); // Add offset to original position
                            CellState state = drawGrid.GetState(position);

                            cellsToCheck.Add(new Cell(position, state)); // Use the tile from the tilemap
                        }
                    }
                }
            }
        }

        return cellsToCheck;
    }
}
