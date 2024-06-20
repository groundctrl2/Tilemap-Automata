using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Normal version of Conway's GOL
public class NormalLife : ILife
{
    private DrawGrid drawGrid;

    public NormalLife(DrawGrid grid)
    {
        drawGrid = grid;
    }

    public HashSet<Cell> Step(HashSet<Cell> currentCells)
    {
        HashSet<Cell> newCells = new HashSet<Cell>();
        HashSet<Cell> cellsToCheck = GetNeighbors(currentCells);

        foreach (Cell cell in cellsToCheck)
        {
            int neighbors = drawGrid.CountNeighborsOfGivenState(cell.position, CellState.alive);

            if (cell.state == CellState.alive && neighbors >= 2 && neighbors <= 3)
            {
                newCells.Add(new Cell(cell.position, CellState.alive));
            }
            else if (cell.state == CellState.dead && neighbors == 3)
            {
                newCells.Add(new Cell(cell.position, CellState.alive));
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

        return cellsToCheck;
    }
}
