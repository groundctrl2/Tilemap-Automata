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
    private List<Vector3Int> takenPredatorPositions;
    private List<Vector3Int> takenPreyPositions;
    private int preyCount = int.MinValue;
    private int predatorCount = 0;

    public HuntingLife(DrawGrid grid)
    {
        drawGrid = grid ?? throw new ArgumentNullException(nameof(grid));
    }

    public HashSet<Cell> Step(HashSet<Cell> currentCells)
    {
        HashSet<Cell> newCells = new HashSet<Cell>();
        takenPredatorPositions = new List<Vector3Int>();
        takenPreyPositions = new List<Vector3Int>();
        int newBabyPreyCount = 0;

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
                    // Hunger death if predator:prey ratio unbalanced
                    if (predatorCount > preyCount / 4 && preyCount > 0 && predatorCount > 1)
                        predatorCount--; // predator dies
                    // Else if predator has prey to hunt
                    else if (preyPositions != null && preyPositions.Count != 0)
                    {
                        Vector3Int preyNearby = GetStateNearby(cell.position, prey);

                        // Eat (Ensure prey is there and position not already taken by predator)
                        if (preyNearby.z != int.MaxValue && !takenPredatorPositions.Contains(preyNearby))
                        {
                            newCells.Add(new Cell(preyNearby, predator));
                            takenPredatorPositions.Add(preyNearby);

                            // Baby predator left in previous place if certain amount of total kills
                            if (UnityEngine.Random.value > .95)
                            {
                                newCells.Add(new Cell(cell.position, predator));
                                predatorCount++;
                                takenPredatorPositions.Add(cell.position);
                            }
                        }
                        // Hunt
                        else
                        {
                            // Get best next position
                            List<Vector3Int> availablePositions = GetAvailablePositions(cell.position, true);
                            Vector3Int bestPosition = GetBestPosition(cell.position, availablePositions, takenPredatorPositions);

                            newCells.Add(new Cell(bestPosition, predator));
                            takenPredatorPositions.Add(bestPosition); // Ensure another predator doesn't take place
                        }
                    }
                    // Stay in place if prey positions not found yet (done during 2nd step)
                    else
                    {
                        newCells.Add(new Cell(cell.position, predator));
                        takenPredatorPositions.Add(cell.position); // Ensure another predator doesn't take place
                    }
                }
                // Prey logic
                else if (cell.state == prey)
                {
                    List<Vector3Int> availablePositions = GetAvailablePositions(cell.position, false);
                    bool predatorNearby = false;

                    // Remove dangerous positions
                    List<Vector3Int> toRemove = new List<Vector3Int>();
                    foreach (Vector3Int position in availablePositions)
                        if (GetStateNearby(position, predator).z != int.MaxValue)
                        {
                            predatorNearby = true;
                            toRemove.Add(position);
                        }
                    foreach (Vector3Int position in toRemove)
                        availablePositions.Remove(position);

                    // If no available positions left, add current position back
                    if (availablePositions.Count == 0)
                        availablePositions.Add(cell.position);

                    // Move to best position or random position depending on whether best is in the same place + small chance of fainting/freezing
                    Vector3Int randomPosition = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];
                    Vector3Int bestPosition = GetBestPosition(cell.position, availablePositions, takenPreyPositions);
                    Vector3Int nextPosition;

                    if (UnityEngine.Random.value > .95)
                        nextPosition = cell.position;
                    else if (bestPosition == cell.position)
                        nextPosition = randomPosition;
                    else
                        nextPosition = bestPosition;

                    // Move
                    newCells.Add(new Cell(nextPosition, prey));
                    takenPreyPositions.Add(nextPosition);

                    // If another prey nearby, no predators nearby, and mostly alone, baby prey is left in previous position (only if prey moves)
                    // Prey count caps at DrawGrid population goal
                    if (GetStateNearby(cell.position, prey).z != int.MaxValue && nextPosition != cell.position)
                        if (availablePositions.Count > 3 && !predatorNearby && preyCount < drawGrid.populationGoal)
                        {
                            newCells.Add(new Cell(cell.position, prey));
                            newBabyPreyCount++;
                        }
                }
                else
                {
                    Debug.Log($"Non predator/prey cell state in currentCells, cell state: {cell.state}");
                }
            }
        }

        UpdatePreyPositionsAndCounts(newCells);
        Debug.Log($"predator count: {predatorCount}, prey count: {preyCount}");
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

    // Clears prey position list and adds new prey positions, also recounts prey and predator counts
    private void UpdatePreyPositionsAndCounts(HashSet<Cell> cells)
    {
        preyPositions.Clear();
        predatorCount = 0;
        preyCount = 0;
        foreach (Cell cell in cells)
        {
            if (cell.state == prey)
            {
                preyPositions.Add(cell.position);
                preyCount++;
            }
            else
                predatorCount++;
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

    // Given the cells current position and a list of available positions, finds the closest prey and returns the available position closest to them.
    private Vector3Int GetBestPosition(Vector3Int currentPosition, List<Vector3Int> availablePositions, List<Vector3Int> takenPositions)
    {
        // Find best target
        Vector3Int target = currentPosition; // Will stay in place if no target found
        double bestDistance = int.MaxValue;
        foreach (Vector3Int position in preyPositions)
        {
            double distance = Math.Sqrt(Math.Pow(currentPosition.x - position.x, 2) + Math.Pow(currentPosition.y - position.y, 2));
            if (bestDistance > distance)
            {
                bestDistance = distance;
                target = position;
            }
        }

        // Find best next position
        Vector3Int bestPosition = currentPosition; // Will stay in place if no best position found
        bestDistance = int.MaxValue;
        foreach (Vector3Int position in availablePositions)
        {
            // Ensure position not already taken by predator and possibly (if search is for prey) prey
            if (!takenPositions.Contains(position) && !takenPredatorPositions.Contains(position))
            {
                double distance = Math.Sqrt(Math.Pow(position.x - target.x, 2) + Math.Pow(position.y - target.y, 2));
                if (bestDistance > distance)
                {
                    bestDistance = distance;
                    bestPosition = position;
                }
            }
        }

        return bestPosition;
    }
}
