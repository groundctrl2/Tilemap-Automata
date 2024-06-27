using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DrawGrid : MonoBehaviour
{
    [SerializeField] public Tilemap tilemap;
    [SerializeField] private Tile[] tiles = new Tile[8];
    [SerializeField] private float updateInterval = 0.05f; // In seconds
    [SerializeField] private Pattern Pattern;

    public int iterations { get; private set; }
    public float time { get; private set; }
    private float startTime;

    private ILife model;
    private HashSet<Cell> cells;
    public int populationGoal { get; private set; }

    // 1st step, no external reference needed (Start opposite)
    private void Awake()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap is not assigned");
        }
        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("Tiles are not assigned or empty");
        }
        if (Pattern == null)
        {
            Debug.LogError("Pattern is not assigned");
        }

        cells = new HashSet<Cell>();
        startTime = Time.time; // Initialize start time
        model = new HuntingLife(this);
        SetPattern(Pattern);
        populationGoal = 15000;
    }

    // Runs Simulate() on enable
    private void OnEnable()
    {
        if (tilemap != null && tiles != null && Pattern != null)
        {
            StartCoroutine(Simulate());
        }
        else
        {
            Debug.LogError("Simulation cannot start. Missing components.");
        }
    }

    // Updates generations at the given updateInterval value (in seconds)
    private IEnumerator Simulate()
    {
        var interval = new WaitForSeconds(updateInterval); // Created once to avoid garbage accumulation
        float currentUpdateInterval = updateInterval;

        yield return interval; // Done to see initial state

        while (enabled)
        {
            UpdateState();

            iterations++;
            time = Time.time - startTime; // Calculate elapsed time

            // Redo if value changes
            if (currentUpdateInterval != updateInterval)
            {
                interval = new WaitForSeconds(updateInterval);
                currentUpdateInterval = updateInterval;
            }

            yield return interval;
        }
    }

    // Get state changes and set into cleared tilemap
    private void UpdateState()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap is null during UpdateState");
            return;
        }

        tilemap.ClearAllTiles(); // Clear previous tiles
        cells = model.Step(cells);

        if (cells == null)
        {
            Debug.LogError("Model step returned null cells");
            return;
        }

        foreach (Cell cell in cells)
        {
            if (cell.position == null)
            {
                Debug.LogError("Cell position is null");
                continue;
            }
            if (tiles[(int)cell.state] == null)
            {
                Debug.Log($"Null cell state: {cell.state}");
                continue;
            }

            tilemap.SetTile(cell.position, tiles[(int)cell.state]);
        }
    }

    // Clear the game
    private void Clear()
    {
        if (tilemap != null)
        {
            tilemap.ClearAllTiles();
        }

        cells = new HashSet<Cell>();
        iterations = 0;
        time = 0f;
    }

    // Counts and returns neighbors of given position with the given state
    public int CountNeighborsOfGivenState(Vector3Int position, CellState state)
    {
        int count = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighborPosition = position + new Vector3Int(x, y, 0); // Add offset to original position
                if (GetState(neighborPosition) == state && (x != 0 || y != 0))
                {
                    count++;
                }
            }
        }

        return count;
    }

    // Returns neighbors of given position
    public CellState[] GetNeighborStates(Vector3Int position)
    {
        CellState[] neighbors = new CellState[8];

        int neighborIndex = 0;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int neighborPosition = position + new Vector3Int(x, y, 0); // Add offset to original position
                if (x != 0 || y != 0)
                {
                    neighbors[neighborIndex] = GetState(neighborPosition);
                    neighborIndex++;
                }
            }
        }

        return neighbors;
    }

    // Get the state of the cell at the given position from the cells hashset
    public CellState GetState(Vector3Int position)
    {
        // Create a temporary Cell object with the desired position
        Cell tempCell = new Cell(position, CellState.dead); // State is irrelevant for search

        if (cells.TryGetValue(tempCell, out Cell foundCell))
        {
            return foundCell.state; // Return the found cell
        }

        return CellState.dead; // Return null/dead if the cell is not found
    }

    // Get the cells hashset
    public HashSet<Cell> GetCells()
    {
        if (cells == null)
        {
            cells = new HashSet<Cell>();
        }

        return cells;
    }

    // Places the given cell pattern in the middle of the grid
    private void SetPattern(Pattern pattern)
    {
        if (pattern == null)
            Debug.Log("Pattern is null.");

        Clear();
        Vector2Int center = pattern.GetCenter();

        for (int i = 0; i < pattern.cells.Length; i++)
        {
            // Cast because SetTile requires Vector3, offset by center point to ensure pattern centered at origin
            Vector3Int position = (Vector3Int)(pattern.cells[i] - center);
            addCurrentCell(position, CellState.alive);
        }
    }

    // Add a cell of the given state at the given position to the current tilemap and hashset
    public void addCurrentCell(Vector3Int position, CellState state)
    {
        cells.Add(new Cell(position, state));
        tilemap.SetTile(position, tiles[(int)state]);
    }

    // Return whether generations per second is slowing more than 95% target generations per second (based off update interval)
    private bool IsSlowing()
    {
        if (iterations == 0 || time == 0)
            return false;
        else
        {
            float actualGenerationsPerSecond = iterations / time;
            float targetGenerationsPerSecond = 1 / updateInterval;
            return actualGenerationsPerSecond < targetGenerationsPerSecond * .75f;
        }
    }
}
