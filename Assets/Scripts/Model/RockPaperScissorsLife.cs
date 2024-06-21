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

    private float birthRate = .95f;
    private float populationCap = float.MaxValue;
    private float populationGoal = float.MaxValue;

    public RockPaperScissorsLife(DrawGrid grid)
    {
        drawGrid = grid;
    }

    public HashSet<Cell> Step(HashSet<Cell> currentCells)
    {
        HashSet<Cell> newCells = new HashSet<Cell>();
        HashSet<Cell> cellsToCheck = GetNeighbors(currentCells);

        int cellCount = currentCells.Count;
        adjustBirthAndDeathRates(cellCount);

        foreach (Cell cell in cellsToCheck)
        {
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

                // Record updates based on number of predator neighbors
                int predatorNeighbors = drawGrid.CountNeighborsOfGivenState(cell.position, predator);
                int sameStateNeighbors = drawGrid.CountNeighborsOfGivenState(cell.position, cell.state);
                int emptyNeighbors = drawGrid.CountNeighborsOfGivenState(cell.position, CellState.dead);

                if (predatorNeighbors > 2)
                    newCells.Add(new Cell(cell.position, predator)); // Predator wins
                else if (predatorNeighbors == sameStateNeighbors)
                    cellCount--; // Tie death
                else
                {
                    if (cellCount >= populationGoal && UnityEngine.Random.value > .9f && emptyNeighbors >= 6 && emptyNeighbors <= 8)
                        cellCount--; // Overpopulation death
                    else
                        newCells.Add(new Cell(cell.position, cell.state)); // Current wins    
                }
            }
            // else if cell is empty, small chance of random state birth or birth if all neighbors alive
            else if (cell.state == CellState.dead)
            {
                if (UnityEngine.Random.value > birthRate || drawGrid.CountNeighborsOfGivenState(cell.position, CellState.dead) == 0)
                    newCells.Add(new Cell(cell.position, usedStates[UnityEngine.Random.Range(0, 3)])); // Random chance of birth
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

    private void adjustBirthAndDeathRates(float cellCount)
    {
        bool isSlowing;

        // Only chance of being true if population is over cap
        if (cellCount < populationCap && populationCap != float.MaxValue)
            isSlowing = false;
        else
            isSlowing = drawGrid.IsSlowing();


        if (isSlowing)
        {
            if (birthRate < .99)
                birthRate *= 1.005f;
            if (drawGrid.time > 10 && populationCap == float.MaxValue)
                populationCap = (int)(cellCount * .75);
        }
        else
        {
            if (birthRate > .94)
                birthRate *= .995f;
        }

        populationGoal = (cellCount + populationCap) / 2;
        Debug.Log($"slowing: {isSlowing}, cellCount: {cellCount}, popCap: {populationCap}, popGoal: {populationGoal} birthRate: {birthRate}");
    }
}
