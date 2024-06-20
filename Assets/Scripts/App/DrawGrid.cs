using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DrawGrid : MonoBehaviour
{
    [SerializeField] public Tilemap tilemap;
    [SerializeField] private Tile[] tiles = new Tile[8];
    [SerializeField] private float updateInterval = 0.02f; // In seconds
    [SerializeField] private Pattern Pattern;

    public int iterations { get; private set; }
    public float time { get; private set; }

    private ILife model;
    private HashSet<Cell> cells;

    // 1st step, no external reference needed (Start opposite)
    private void Awake()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap is not assigned!");
        }
        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("Tiles are not assigned or empty!");
        }
        if (Pattern == null)
        {
            Debug.LogError("Pattern is not assigned!");
        }

        //model = new NormalLife(this);
        model = new GenSpectrumLife(this);
        cells = new HashSet<Cell>();
        SetPattern(Pattern);
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
            time += updateInterval;

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

        foreach (Cell cell in cells)
        {
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
    private void SetPattern(Pattern Pattern)
    {
        Clear();

        Vector2Int center = Pattern.GetCenter();

        for (int i = 0; i < Pattern.cells.Length; i++)
        {
            // Cast because SetTile requires Vector3, offset by center point to ensure pattern centered at origin
            Vector3Int position = (Vector3Int)(Pattern.cells[i] - center);
            addCurrentCell(position, CellState.alive);
        }
    }

    // Add a cell of the given state at the given position to the current tilemap and hashset
    public void addCurrentCell(Vector3Int position, CellState state)
    {
        cells.Add(new Cell(position, state));
        tilemap.SetTile(position, tiles[(int)state]);
    }
}
