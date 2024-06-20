using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generational Color Life. Conway's Game of Life, but dead cells change color a few times before fully dying out (No dead cells affect the game).
public class ColorLife : ILife
{
    private DrawGrid drawGrid;

    public ColorLife(DrawGrid grid)
    {
        drawGrid = grid;
    }

    public HashSet<Cell> Step(HashSet<Cell> currentCells)
    {
        HashSet<Cell> newCells = new HashSet<Cell>();
        HashSet<Cell> cellsToCheck = GetCellsToCheck(currentCells);

        foreach (Cell cell in cellsToCheck)
        {
            int neighbors = drawGrid.CountNeighborsOfGivenState(cell.position, CellState.alive);

            if (cell.state == CellState.alive)
            {
                if (neighbors >= 2 && neighbors <= 3)
                {
                    newCells.Add(new Cell(cell.position, CellState.alive));
                }
                else
                {
                    newCells.Add(new Cell(cell.position, CellState.stage1));
                }
            }
            else if (neighbors == 3)
            {
                newCells.Add(new Cell(cell.position, CellState.alive));
            }
            // Begin "generational spectrum coloring" for dead
            else
            {
                if (cell.state == CellState.stage1)
                    newCells.Add(new Cell(cell.position, CellState.stage2));
                else if (cell.state == CellState.stage2)
                    newCells.Add(new Cell(cell.position, CellState.stage3));
                else if (cell.state == CellState.stage3)
                    newCells.Add(new Cell(cell.position, CellState.stage4));
                else if (cell.state == CellState.stage4)
                    newCells.Add(new Cell(cell.position, CellState.stage5));
            }
        }

        return newCells;
    }

    // Builds and returns a hashset containing the current cells as well as their neighboring cells to be checked by game logic
    private HashSet<Cell> GetCellsToCheck(HashSet<Cell> currentCells)
    {
        HashSet<Cell> cellsToCheck = new HashSet<Cell>();

        if (currentCells.Count != 0 && currentCells != null)
        {
            foreach (Cell cell in currentCells)
            {
                if (cell.state == CellState.alive)
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
                // Else dead, meaning no neighbors need to be added
                else
                {
                    cellsToCheck.Add(cell);
                }
            }
        }

        return cellsToCheck;
    }
}
