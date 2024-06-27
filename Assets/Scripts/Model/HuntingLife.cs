using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HuntingLife : ILife
{
    private DrawGrid drawGrid;
    private bool firstStep = true;
    private CellState predator = CellState.stage1;
    private CellState prey = CellState.alive;
    private List<Vector3Int> preyPositions = new List<Vector3Int>();
    private int killCount = 0;
    private int predatorCount = 0;

    public HuntingLife(DrawGrid grid)
    {
        drawGrid = grid ?? throw new ArgumentNullException(nameof(grid));
    }

    public HashSet<Cell> Step(HashSet<Cell> currentCells)
    {
        HashSet<Cell> newCells = new HashSet<Cell>();

        foreach (Cell cell in currentCells)
        {
            // Set first cell as predator if first step
            if (firstStep)
            {
                firstStep = false;
                newCells.Add(new Cell(cell.position, predator));
                predatorCount++;
            }
            // Normal Game logic
            else
            {
                // Predator logic
                if (cell.state == predator)
                {
                    if (preyPositions != null && preyPositions.Count != 0)
                    {
                        // Eat
                        Vector3Int preyNearby = GetStateNearby(cell.position, prey);
                        if (preyNearby.z != int.MaxValue)
                        {
                            newCells.Add(new Cell(preyNearby, predator));
                            killCount++;

                            // Baby predator left in previous place if certain amount of total kills
                            if (killCount / 25 > predatorCount)
                            {
                                newCells.Add(new Cell(cell.position, predator));
                                predatorCount++;
                            }
                        }
                        // Hunt
                        else
                        {
                            // Find best target
                            Vector3Int target = cell.position; // Will stay in place if no target found
                            double bestDistance = int.MaxValue;
                            foreach (Vector3Int position in preyPositions)
                            {
                                double distance = Math.Sqrt(Math.Pow(cell.position.x - position.x, 2) + Math.Pow(cell.position.y - position.y, 2));
                                if (bestDistance > distance)
                                {
                                    bestDistance = distance;
                                    target = position;
                                }
                            }

                            // Find best next position
                            List<Vector3Int> availablePositions = GetAvailablePositions(cell.position, true);
                            Vector3Int bestPosition = cell.position; // Will stay in place if no best position found
                            bestDistance = int.MaxValue;
                            foreach (Vector3Int position in availablePositions)
                            {
                                double distance = Math.Sqrt(Math.Pow(position.x - target.x, 2) + Math.Pow(position.y - target.y, 2));
                                if (bestDistance > distance)
                                {
                                    bestDistance = distance;
                                    bestPosition = position;
                                }
                            }

                            newCells.Add(new Cell(bestPosition, predator));
                        }
                    }
                    // Stay in place if prey positions not found yet (done during 2nd step)
                    else
                        newCells.Add(new Cell(cell.position, predator));
                }
                // Prey logic
                else if (cell.state == prey)
                {
                    List<Vector3Int> availablePositions = GetAvailablePositions(cell.position, false);
                    Vector3Int randomPosition = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];
                    newCells.Add(new Cell(randomPosition, prey));

                    // If another prey nearby and mostly alone, chance of baby prey is left in previous position (only if prey moves)
                    if (GetStateNearby(cell.position, prey).z != int.MaxValue && randomPosition != cell.position && availablePositions.Count == 3 && UnityEngine.Random.value > .35)
                        newCells.Add(new Cell(cell.position, prey));
                }
                else
                {
                    Debug.Log($"Non predator/prey cell state in currentCells, cell state: {cell.state}");
                }
            }
        }

        UpdatePreyPositions(newCells);
        return newCells;
    }

    // Returns list of available next positions based on whether cell is predator or prey (prey has only orthogonal neighboring positions, predator all 8)
    private List<Vector3Int> GetAvailablePositions(Vector3Int position, bool isPredator)
    {
        List<Vector3Int> availablePositions = new List<Vector3Int> { position }; // Add current position in case best option/no others available

        // Get free orthogonal neighbor positions
        Vector3Int[] orthogonalOffsets = { new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, -1, 0), new Vector3Int(0, 1, 0) };
        foreach (Vector3Int offset in orthogonalOffsets)
        {
            Vector3Int neighborPosition = position + offset;
            if (drawGrid.GetState(neighborPosition) == CellState.dead)
                availablePositions.Add(neighborPosition);
        }

        // Predators only
        if (isPredator)
        {
            // Get free diagonal neighbor positions
            Vector3Int[] diagonalOffsets = { new Vector3Int(-1, -1, 0), new Vector3Int(-1, 1, 0), new Vector3Int(1, -1, 0), new Vector3Int(1, 1, 0) };
            foreach (Vector3Int offset in diagonalOffsets)
            {
                Vector3Int neighborPosition = position + offset;
                if (drawGrid.GetState(neighborPosition) == CellState.dead)
                    availablePositions.Add(neighborPosition);
            }
        }

        return availablePositions;
    }

    // Clears prey position list and adds new prey positions
    private void UpdatePreyPositions(HashSet<Cell> cells)
    {
        preyPositions.Clear();
        foreach (Cell cell in cells)
        {
            if (cell.state == prey)
                preyPositions.Add(cell.position);
        }
    }

    // Returns position of prey/predator/other state that's an orthogonal move away if there's one nearby, or a Vector3Int with a max z value otherwise
    private Vector3Int GetStateNearby(Vector3Int position, CellState state)
    {
        Vector3Int stateNearby = new Vector3Int(0, 0, int.MaxValue);

        // Check each orthogonal neighbor position
        Vector3Int[] orthogonalOffsets = { new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, -1, 0), new Vector3Int(0, 1, 0) };
        foreach (Vector3Int offset in orthogonalOffsets)
        {
            Vector3Int neighborPosition = position + offset;
            if (drawGrid.GetState(neighborPosition) == state)
                stateNearby = neighborPosition;
        }

        return stateNearby;
    }
}
