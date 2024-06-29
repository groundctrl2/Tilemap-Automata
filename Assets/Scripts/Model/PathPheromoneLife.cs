using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPheromoneLife : ILife
{
    private DrawGrid drawGrid;
    private int xBound = 15;
    private int yBound = 15;
    private System.Random random = new();

    public PathPheromoneLife(DrawGrid grid)
    {
        drawGrid = grid;
    }

    public HashSet<Cell> Step(HashSet<Cell> currentCells)
    {
        HashSet<Cell> newCells = new HashSet<Cell>();
        List<Vector3Int> takenPositions = new List<Vector3Int>();
        int aliveCount = 0;

        // Alive cell logic
        foreach (Cell cell in currentCells)
        {
            if (cell.state == CellState.alive)
            {
                aliveCount++;
                Vector3Int nextPosition = new Vector3Int(0, 0, 0);
                List<Vector3Int> nextPositions = new List<Vector3Int>(); // best options list
                int nextPositionPriority = 0;
                List<Vector3Int> availablePositions = GetAvailablePositions(cell.position, takenPositions);
                Vector3Int emptyPosition = cell.position;

                // Get neighboring position with the highest priority
                foreach (Vector3Int neighborPosition in availablePositions)
                {
                    CellState neighborState = drawGrid.GetState(neighborPosition);
                    int neighborPositionPriority = GetPriority(neighborState);
                    if (!takenPositions.Contains(neighborPosition))
                    {
                        // If better, clear next positions options list
                        if (neighborPositionPriority > nextPositionPriority)
                        {
                            nextPositions.Clear();
                            nextPositions.Add(neighborPosition);
                            nextPositionPriority = neighborPositionPriority;
                        }
                        // Else if equal, add to next positions options list
                        else if (neighborPositionPriority == nextPositionPriority)
                            nextPositions.Add(neighborPosition);
                    }

                    // Store a dead/empty space for random chance of moving
                    if (neighborState == CellState.dead)
                        emptyPosition = neighborPosition;
                }

                // randomly choose from options list a next position
                if (nextPositions.Count != 0)
                {
                    int randomIndex = random.Next(nextPositions.Count);
                    nextPosition = nextPositions[randomIndex];
                }

                // Random chance of wandering to dead/empty space instead
                if (!takenPositions.Contains(emptyPosition) && UnityEngine.Random.value > drawGrid.wanderChance)
                    nextPosition = emptyPosition;

                if (nextPosition == new Vector3Int(0, 0, 0))
                    Debug.Log("nextPosition never changed");

                // Move to next position
                newCells.Add(new Cell(nextPosition, CellState.alive));
                takenPositions.Add(nextPosition); // Ensure position isn't taken twice
            }
        }

        // Pheromone path spectrum coloring for previously alive cells (separate foreach to avoid coloring overtaking alive cells)
        foreach (Cell cell in currentCells)
        {
            if (cell.state != CellState.dead && !takenPositions.Contains(cell.position))
            {
                if (cell.state == CellState.alive)
                    newCells.Add(new Cell(cell.position, CellState.stage1));
                else if (cell.state == CellState.stage1)
                    newCells.Add(new Cell(cell.position, CellState.stage2));
                else if (cell.state == CellState.stage2)
                    newCells.Add(new Cell(cell.position, CellState.stage3));
                else if (cell.state == CellState.stage3)
                    newCells.Add(new Cell(cell.position, CellState.stage4));
                else if (cell.state == CellState.stage4)
                    newCells.Add(new Cell(cell.position, CellState.stage5));
            }
        }

        //Debug.Log($"alive count: {aliveCount}");
        return newCells;
    }

    // Returns list of orthogonal positions surrounding cell that aren't already taken by alive cells in the next step
    private List<Vector3Int> GetAvailablePositions(Vector3Int position, List<Vector3Int> takenPositions)
    {
        List<Vector3Int> availablePositions = new List<Vector3Int>();

        // Get free orthogonal neighbor positions
        Vector3Int[] orthogonalOffsets = { new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, -1, 0), new Vector3Int(0, 1, 0) };
        foreach (Vector3Int offset in orthogonalOffsets)
        {
            Vector3Int neighborPosition = position + offset;
            // Ensure position doesn't exit bounds
            if (neighborPosition.x < xBound && neighborPosition.x > -xBound && neighborPosition.y < yBound && neighborPosition.y > -yBound)
                if (!takenPositions.Contains(neighborPosition))
                    availablePositions.Add(neighborPosition);
        }

        return availablePositions;
    }

    // Returns priority score based on CellState
    private int GetPriority(CellState state)
    {
        int priority;

        if (state == CellState.alive)
            priority = 6;
        else if (state == CellState.stage2)
            priority = 5;
        else if (state == CellState.stage3)
            priority = 4;
        else if (state == CellState.stage4)
            priority = 3;
        else if (state == CellState.stage5)
            priority = 2;
        else if (state == CellState.dead)
            priority = 1;
        else
            priority = 0;

        return priority;
    }
}