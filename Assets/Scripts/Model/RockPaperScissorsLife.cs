using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

// Population growth rock paper scissor implementation
public class RockPaperScissorsLife : ILife
{
    private DrawGrid drawGrid;
    private CellState[] usedStates = { CellState.stage1, CellState.stage3, CellState.stage4 };
    private int populationCap = 5000;

    public RockPaperScissorsLife(DrawGrid grid)
    {
        drawGrid = grid;
    }

    public HashSet<Cell> Step(HashSet<Cell> currentCells)
    {
        HashSet<Cell> newCells = new HashSet<Cell>();
        HashSet<Cell> cellsToCheck = GetNeighbors(currentCells);

        int cellCount = currentCells.Count;

        Debug.Log($"cellCount: {cellCount}");

        foreach (Cell cell in cellsToCheck)
        {
            // If not dead or non R/P/S
            if (usedStates.Contains(cell.state))
            {
                // Get predator CellState
                CellState predator;
                if (cell.state == usedStates[0])
                    predator = usedStates[1];
                else if (cell.state == usedStates[1])
                    predator = usedStates[2];
                else // if (cell.state == usedStates[2])
                    predator = usedStates[0];

                int predatorNeighbors = drawGrid.CountNeighborsOfGivenState(cell.position, predator);
                int emptyNeighbors = drawGrid.CountNeighborsOfGivenState(cell.position, CellState.dead);

                // Predator wins
                if (predatorNeighbors > 2)
                    newCells.Add(new Cell(cell.position, predator));
                else if (emptyNeighbors == 6)
                    cellCount--;
                // Current wins
                else
                    newCells.Add(new Cell(cell.position, cell.state));
            }
            // else if cell is empty, possible birth
            else if (cell.state == CellState.dead)
            {
                int emptyNeighbors = drawGrid.CountNeighborsOfGivenState(cell.position, CellState.dead);
                int rockNeighbors = drawGrid.CountNeighborsOfGivenState(cell.position, usedStates[0]);
                int paperNeighbors = drawGrid.CountNeighborsOfGivenState(cell.position, usedStates[1]);
                int scissorsNeighbors = 8 - emptyNeighbors - rockNeighbors - paperNeighbors;

                if (cellCount < populationCap && emptyNeighbors > 6)
                {
                    // if only rock neighbors, spawn paper
                    if (emptyNeighbors + rockNeighbors == 7 && scissorsNeighbors == 1)
                        newCells.Add(new Cell(cell.position, usedStates[1]));
                    // if only paper neighbors, spawn scissors
                    if (emptyNeighbors + paperNeighbors == 7 && rockNeighbors == 1)
                        newCells.Add(new Cell(cell.position, usedStates[2]));
                    // if only scissors neighbors, spawn rock
                    if (emptyNeighbors + scissorsNeighbors == 7 && paperNeighbors == 1)
                        newCells.Add(new Cell(cell.position, usedStates[0]));
                }
                else if (cellCount > populationCap && emptyNeighbors <= 3)
                {
                    if (rockNeighbors > paperNeighbors && rockNeighbors > scissorsNeighbors)
                        newCells.Add(new Cell(cell.position, usedStates[1]));
                    else if (paperNeighbors > rockNeighbors && paperNeighbors > scissorsNeighbors)
                        newCells.Add(new Cell(cell.position, usedStates[2]));
                    else if (scissorsNeighbors > rockNeighbors && scissorsNeighbors > paperNeighbors)
                        newCells.Add(new Cell(cell.position, usedStates[0]));
                }
            }
            // else cell is CellState not recognized by model (pattern/user added), change to random used state
            else
                newCells.Add(new Cell(cell.position, usedStates[UnityEngine.Random.Range(0, 3)]));
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
