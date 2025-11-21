using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int gridWidth = 8;
    public int gridHeight = 8;
    public float cellSize = 1f;
    public GameObject cellPrefab;

    private GridCell[,] grid;
    private List<GridCell> highlightedCells = new List<GridCell>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        GenerateGrid();
    }

    void GenerateGrid()
    {
        Debug.Log($"Generating grid: {gridWidth}x{gridHeight}");
        grid = new GridCell[gridWidth, gridHeight];

        // Calculate offset to center the grid
        Vector3 offset = new Vector3(
            -(gridWidth * cellSize) / 2f + cellSize / 2f,
            -(gridHeight * cellSize) / 2f + cellSize / 2f,
            0
        );

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = new Vector3(x * cellSize, y * cellSize, 0) + offset;
                GameObject cellObj = CreateCell(worldPos);

                GridCell cell = cellObj.GetComponent<GridCell>();
                cell.gridPosition = new Vector2Int(x, y);
                grid[x, y] = cell;
            }
        }

        Debug.Log($"Grid generated: {grid.Length} cells created");
    }

    GameObject CreateCell(Vector3 position)
    {
        // Create a simple quad sprite if no prefab is assigned
        if (cellPrefab == null)
        {
            GameObject cell = new GameObject("GridCell");
            cell.transform.position = position;
            cell.transform.parent = transform;

            // Add sprite renderer for visual
            SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.sortingOrder = -1; // Behind units

            // Add box collider for mouse interaction
            BoxCollider2D collider = cell.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(cellSize * 0.95f, cellSize * 0.95f);

            // Add GridCell component
            cell.AddComponent<GridCell>();

            return cell;
        }
        else
        {
            GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
            return cell;
        }
    }

    Sprite CreateSquareSprite()
    {
        // Create a simple square texture
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
    }

    public GridCell GetCell(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return grid[x, y];
        }
        return null;
    }

    public GridCell GetCell(Vector2Int position)
    {
        return GetCell(position.x, position.y);
    }

    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        GridCell cell = GetCell(gridPos);
        return cell != null ? cell.transform.position : Vector3.zero;
    }

    public Vector2Int GetGridPosition(Vector3 worldPos)
    {
        // Calculate offset
        Vector3 offset = new Vector3(
            -(gridWidth * cellSize) / 2f + cellSize / 2f,
            -(gridHeight * cellSize) / 2f + cellSize / 2f,
            0
        );

        Vector3 localPos = worldPos - offset;
        int x = Mathf.RoundToInt(localPos.x / cellSize);
        int y = Mathf.RoundToInt(localPos.y / cellSize);

        return new Vector2Int(x, y);
    }

    public List<GridCell> GetNeighbors(Vector2Int position, int range = 1)
    {
        List<GridCell> neighbors = new List<GridCell>();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = position.x + x;
                int checkY = position.y + y;

                GridCell cell = GetCell(checkX, checkY);
                if (cell != null)
                {
                    neighbors.Add(cell);
                }
            }
        }

        return neighbors;
    }

    public List<GridCell> GetCellsInRange(Vector2Int position, int range, bool walkableOnly = false)
    {
        List<GridCell> cellsInRange = new List<GridCell>();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                // Use Manhattan distance for grid-based movement
                if (Mathf.Abs(x) + Mathf.Abs(y) <= range)
                {
                    int checkX = position.x + x;
                    int checkY = position.y + y;

                    GridCell cell = GetCell(checkX, checkY);
                    if (cell != null)
                    {
                        if (!walkableOnly || (cell.isWalkable && !cell.isOccupied))
                        {
                            cellsInRange.Add(cell);
                        }
                    }
                }
            }
        }

        return cellsInRange;
    }

    public void HighlightCells(List<GridCell> cells, bool isAttackRange = false)
    {
        ClearHighlights();
        
        foreach (GridCell cell in cells)
        {
            if (isAttackRange)
            {
                cell.HighlightAsAttackRange();
            }
            else
            {
                cell.HighlightAsMovable();
            }
            highlightedCells.Add(cell);
        }
    }

    public void ClearHighlights()
    {
        foreach (GridCell cell in highlightedCells)
        {
            cell.ResetHighlight();
        }
        highlightedCells.Clear();
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridWidth && 
               position.y >= 0 && position.y < gridHeight;
    }
}