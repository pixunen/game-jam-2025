using UnityEngine;

public class IceTrap : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public int rootDuration = 2; // turns the enemy will be rooted
    private bool hasTriggered = false;

    public void Initialize(Vector2Int position)
    {
        GridPosition = position;

        // Set the grid cell to show trap color
        if (GridManager.Instance != null)
        {
            GridCell cell = GridManager.Instance.GetCell(position);
            if (cell != null)
            {
                cell.SetTrap(gameObject);
            }

            // Position this GameObject at the cell (invisible, just for tracking)
            transform.position = GridManager.Instance.GetWorldPosition(position);
        }

        Debug.Log($"Ice Trap placed at {position}");
    }

    public bool TryTrigger(GameObject unit)
    {
        if (hasTriggered)
        {
            return false;
        }

        // Only trigger on enemies
        EnemyController enemy = unit.GetComponent<EnemyController>();
        if (enemy != null)
        {
            Debug.Log($"Ice Trap triggered by {unit.name}! Rooting for {rootDuration} turns.");

            // Apply root effect
            StatusEffect rootEffect = new StatusEffect(StatusEffectType.Rooted, rootDuration);
            enemy.ApplyStatusEffect(rootEffect);

            hasTriggered = true;

            // Clear trap color from grid cell
            if (GridManager.Instance != null)
            {
                GridCell cell = GridManager.Instance.GetCell(GridPosition);
                if (cell != null)
                {
                    cell.ClearTrap();
                }
            }

            // Destroy trap after trigger
            Destroy(gameObject, 0.5f);

            return true;
        }

        return false;
    }

    public bool IsTriggered()
    {
        return hasTriggered;
    }

    void OnDestroy()
    {
        // Make sure trap color is cleared when destroyed
        if (!hasTriggered && GridManager.Instance != null)
        {
            GridCell cell = GridManager.Instance.GetCell(GridPosition);
            if (cell != null && cell.trapObject == gameObject)
            {
                cell.ClearTrap();
            }
        }
    }
}