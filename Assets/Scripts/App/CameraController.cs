using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private DrawGrid drawGrid;
    private HashSet<Cell> cells;
    private Camera mainCamera;

    private float targetCameraSize;
    private Vector3 targetPosition;

    public float moveSpeed = 5f;
    public float zoomSpeed = 5f;

    void Start()
    {
        drawGrid = FindObjectOfType<DrawGrid>();
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        cells = drawGrid.GetCells();
        SetCameraAdjustmentInfo();

        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetCameraSize, zoomSpeed * Time.deltaTime);
    }

    public void SetCameraAdjustmentInfo()
    {
        if (cells == null || cells.Count == 0)
        {
            targetPosition = new Vector3Int(0, 0, -10);
            targetCameraSize = 15;
            cells = new HashSet<Cell>();
        }

        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        // Get bounds for all cells
        foreach (Cell cell in cells)
        {
            if (cell.state != CellState.dead)
            {
                Vector3Int position = cell.position;
                min.x = Mathf.Min(position.x, min.x);
                min.y = Mathf.Min(position.y, min.y);
                max.x = Mathf.Max(position.x, max.x);
                max.y = Mathf.Max(position.y, max.y);
            }
        }

        float width = max.x - min.x;
        float height = max.y - min.y;
        targetCameraSize = (Mathf.Max(width, height) / 2) + 10;

        targetPosition = new Vector3Int((min.x + max.x) / 2, (min.y + max.y) / 2, -10);
    }
}
