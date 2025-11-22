using UnityEngine;
using System.Collections.Generic;

public class TelegraphVisualizer : MonoBehaviour
{
    public static TelegraphVisualizer Instance { get; private set; }

    [Header("Telegraph Colors")]
    public Color attackTelegraphColor = new Color(1f, 0.2f, 0.2f, 0.4f); // Red
    public Color moveTelegraphColor = new Color(1f, 1f, 0.3f, 0.3f); // Yellow

    private Dictionary<Vector2Int, GameObject> telegraphIndicators = new Dictionary<Vector2Int, GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Subscribe to turn events
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart += UpdateTelegraphs;
            TurnManager.Instance.OnEnemyTurnStart += UpdateTelegraphs;
        }

        // Initial update
        UpdateTelegraphs();
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart -= UpdateTelegraphs;
            TurnManager.Instance.OnEnemyTurnStart -= UpdateTelegraphs;
        }
    }

    public void UpdateTelegraphs()
    {
        // Clear existing indicators
        ClearAllTelegraphs();

        // Find all enemies
        EnemyController[] enemies = GameObject.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        foreach (EnemyController enemy in enemies)
        {
            if (enemy.nextAction != null && enemy.nextAction.IsValid())
            {
                ShowTelegraph(enemy.nextAction);
            }
        }
    }

    private void ShowTelegraph(TelegraphedAction action)
    {
        if (action == null || !action.IsValid()) return;

        Vector2Int targetPos = action.targetPosition;
        Color indicatorColor;

        switch (action.actionType)
        {
            case TelegraphActionType.Attack:
                indicatorColor = attackTelegraphColor;
                break;
            case TelegraphActionType.Move:
                indicatorColor = moveTelegraphColor;
                break;
            default:
                return; // Don't show indicator for unknown types
        }

        // Create indicator at target position
        CreateTelegraphIndicator(targetPos, indicatorColor);
    }

    private void CreateTelegraphIndicator(Vector2Int gridPos, Color color)
    {
        // Don't create duplicate indicators
        if (telegraphIndicators.ContainsKey(gridPos))
        {
            // Update color if a stronger telegraph exists (attack > move)
            SpriteRenderer existingRenderer = telegraphIndicators[gridPos].GetComponent<SpriteRenderer>();
            if (existingRenderer != null && color == attackTelegraphColor)
            {
                existingRenderer.color = color; // Attack takes priority
            }
            return;
        }

        // Create indicator GameObject
        GameObject indicator = new GameObject($"Telegraph_{gridPos.x}_{gridPos.y}");
        indicator.transform.parent = transform;

        // Set position
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPos);
        indicator.transform.position = worldPos;

        // Add sprite renderer
        SpriteRenderer spriteRenderer = indicator.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = -2; // Below everything else

        // Create a simple square sprite
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        spriteRenderer.color = color;

        // Store reference
        telegraphIndicators[gridPos] = indicator;
    }

    private void ClearAllTelegraphs()
    {
        foreach (var indicator in telegraphIndicators.Values)
        {
            if (indicator != null)
            {
                Destroy(indicator);
            }
        }
        telegraphIndicators.Clear();
    }

    // Public method to force refresh (useful when enemies spawn/die)
    public void RefreshTelegraphs()
    {
        UpdateTelegraphs();
    }
}