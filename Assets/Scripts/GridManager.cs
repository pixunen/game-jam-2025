using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int gridWidth = 8;
    public int gridHeight = 8;
    public float cellSize = 1f;
    public GameObject cellPrefab;

    [Header("Grid Expansion")]
    public bool enableExpansion = true;
    public int expansionTurnInterval = 10;
    public int expansionIncrement = 8;

    [Header("Obstacle Generation")]
    public bool generateObstacles = true;
    public int minObstaclePatterns = 2;
    public int maxObstaclePatterns = 5;

    private GridCell[,] grid;
    private int turnCount = 0;
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

    public void OnTurnEnd()
    {
        turnCount++;
        Debug.Log($"Turn {turnCount} ended.");
        if (enableExpansion && turnCount > 0 && turnCount % expansionTurnInterval == 0)
        {
            int newSize = gridWidth + expansionIncrement;
            ExpandGrid(newSize, newSize);
        }
    }

    public void ExpandGrid(int newWidth, int newHeight)
    {
        if (newWidth <= gridWidth && newHeight <= gridHeight)
        {
            Debug.LogWarning("New grid size must be larger than the current size.");
            return;
        }

        Debug.Log($"Expanding grid from {gridWidth}x{gridHeight} to {newWidth}x{newHeight}");

        GridCell[,] oldGrid = grid;
        int oldWidth = gridWidth;
        int oldHeight = gridHeight;

        // Update grid dimensions
        gridWidth = newWidth;
        gridHeight = newHeight;
        grid = new GridCell[gridWidth, gridHeight];

        // Calculate new offset for centering
        Vector3 offset = new Vector3(
            -(gridWidth * cellSize) / 2f + cellSize / 2f,
            -(gridHeight * cellSize) / 2f + cellSize / 2f,
            0
        );

        int xOffset = (newWidth - oldWidth) / 2;
        int yOffset = (newHeight - oldHeight) / 2;

        // Place old grid in the center of the new grid and create new cells
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPos = new Vector3(x * cellSize, y * cellSize, 0) + offset;

                int oldX = x - xOffset;
                int oldY = y - yOffset;

                if (oldX >= 0 && oldX < oldWidth && oldY >= 0 && oldY < oldHeight)
                {
                    // This is an existing cell from the old grid
                    GridCell cell = oldGrid[oldX, oldY];
                    cell.transform.position = worldPos;
                    cell.gridPosition = new Vector2Int(x, y);
                    grid[x, y] = cell;

                    // Move any objects on the cell
                    if (cell.occupyingUnit != null)
                    {
                        cell.occupyingUnit.transform.position = worldPos;
                    }
                    if (cell.powerUpObject != null)
                    {
                        cell.powerUpObject.transform.position = worldPos;
                    }
                }
                else
                {
                    // This is a new cell
                    GameObject cellObj = CreateCell(worldPos);
                    GridCell cell = cellObj.GetComponent<GridCell>();
                    cell.gridPosition = new Vector2Int(x, y);
                    grid[x, y] = cell;
                }
            }
        }

        // Generate obstacle patterns in the newly expanded areas
        GenerateObstaclePatternsInNewArea(oldWidth, oldHeight);

        Debug.Log($"Grid expanded. New size: {gridWidth}x{gridHeight}");
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

    // Obstacle Pattern Generation
    void GenerateObstaclePatternsInNewArea(int oldWidth, int oldHeight)
    {
        if (!generateObstacles) return;

        int numPatterns = Random.Range(minObstaclePatterns, maxObstaclePatterns + 1);
        Debug.Log($"Generating {numPatterns} obstacle patterns in expanded area");

        for (int i = 0; i < numPatterns; i++)
        {
            // Choose a random pattern type
            int patternType = Random.Range(0, 5);

            // Find a position in the new area (border regions)
            Vector2Int patternOrigin = GetRandomPositionInNewArea(oldWidth, oldHeight);

            if (patternOrigin.x < 0 || patternOrigin.y < 0) continue; // Invalid position

            switch (patternType)
            {
                case 0:
                    CreateWallPattern(patternOrigin, Random.Range(0, 2) == 0); // Random horizontal/vertical
                    break;
                case 1:
                    CreateLShapePattern(patternOrigin);
                    break;
                case 2:
                    CreatePlusPattern(patternOrigin);
                    break;
                case 3:
                    CreateClusterPattern(patternOrigin);
                    break;
                case 4:
                    CreateZigZagPattern(patternOrigin, Random.Range(0, 2) == 0);
                    break;
            }
        }
    }

    Vector2Int GetRandomPositionInNewArea(int oldWidth, int oldHeight)
    {
        int xOffset = (gridWidth - oldWidth) / 2;
        int yOffset = (gridHeight - oldHeight) / 2;

        // Choose a random border region (top, bottom, left, right)
        int region = Random.Range(0, 4);
        Vector2Int position = new Vector2Int(-1, -1);

        switch (region)
        {
            case 0: // Top border
                position = new Vector2Int(
                    Random.Range(0, gridWidth),
                    Random.Range(oldHeight + yOffset, gridHeight)
                );
                break;
            case 1: // Bottom border
                position = new Vector2Int(
                    Random.Range(0, gridWidth),
                    Random.Range(0, yOffset)
                );
                break;
            case 2: // Left border
                position = new Vector2Int(
                    Random.Range(0, xOffset),
                    Random.Range(0, gridHeight)
                );
                break;
            case 3: // Right border
                position = new Vector2Int(
                    Random.Range(oldWidth + xOffset, gridWidth),
                    Random.Range(0, gridHeight)
                );
                break;
        }

        return position;
    }

    void CreateWallPattern(Vector2Int origin, bool horizontal)
    {
        int length = Random.Range(3, 7);

        for (int i = 0; i < length; i++)
        {
            Vector2Int pos = horizontal
                ? new Vector2Int(origin.x + i, origin.y)
                : new Vector2Int(origin.x, origin.y + i);

            SetCellAsObstacle(pos);
        }
    }

    void CreateLShapePattern(Vector2Int origin)
    {
        int armLength = Random.Range(2, 5);
        bool flipHorizontal = Random.Range(0, 2) == 0;
        bool flipVertical = Random.Range(0, 2) == 0;

        int xDir = flipHorizontal ? -1 : 1;
        int yDir = flipVertical ? -1 : 1;

        // Horizontal arm
        for (int i = 0; i < armLength; i++)
        {
            SetCellAsObstacle(new Vector2Int(origin.x + i * xDir, origin.y));
        }

        // Vertical arm
        for (int i = 0; i < armLength; i++)
        {
            SetCellAsObstacle(new Vector2Int(origin.x, origin.y + i * yDir));
        }
    }

    void CreatePlusPattern(Vector2Int origin)
    {
        int armLength = Random.Range(1, 4);

        // Center
        SetCellAsObstacle(origin);

        // Four arms
        for (int i = 1; i <= armLength; i++)
        {
            SetCellAsObstacle(new Vector2Int(origin.x + i, origin.y)); // Right
            SetCellAsObstacle(new Vector2Int(origin.x - i, origin.y)); // Left
            SetCellAsObstacle(new Vector2Int(origin.x, origin.y + i)); // Up
            SetCellAsObstacle(new Vector2Int(origin.x, origin.y - i)); // Down
        }
    }

    void CreateClusterPattern(Vector2Int origin)
    {
        int clusterSize = Random.Range(4, 8);

        for (int i = 0; i < clusterSize; i++)
        {
            Vector2Int offset = new Vector2Int(
                Random.Range(-2, 3),
                Random.Range(-2, 3)
            );
            SetCellAsObstacle(origin + offset);
        }
    }

    void CreateZigZagPattern(Vector2Int origin, bool startHorizontal)
    {
        int segments = Random.Range(2, 4);
        int segmentLength = Random.Range(2, 4);
        Vector2Int current = origin;

        for (int seg = 0; seg < segments; seg++)
        {
            bool isHorizontal = startHorizontal ? (seg % 2 == 0) : (seg % 2 == 1);
            int direction = Random.Range(0, 2) == 0 ? 1 : -1;

            for (int i = 0; i < segmentLength; i++)
            {
                SetCellAsObstacle(current);

                if (isHorizontal)
                {
                    current.x += direction;
                }
                else
                {
                    current.y += direction;
                }
            }
        }
    }

    void SetCellAsObstacle(Vector2Int position)
    {
        if (!IsValidPosition(position)) return;

        GridCell cell = GetCell(position);
        if (cell != null && !cell.isOccupied)
        {
            cell.SetWalkable(false);
        }
    }
}