using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool isWalkable = true;
    public bool isOccupied = false;
    public GameObject occupyingUnit = null;
    public GameObject powerUpObject = null;
    public GameObject trapObject = null;

    private SpriteRenderer spriteRenderer;
    private Color defaultColor = new(0.9f, 0.9f, 0.9f, 1f); // Light gray
    private Color highlightColor = new(0.5f, 1f, 0.5f, 0.7f); // Green highlight
    private Color attackRangeColor = new(1f, 0.5f, 0.5f, 0.7f); // Red for attack range
    private Color blockedColor = new(0.5f, 0.5f, 0.5f, 1f); // Dark gray for blocked
    private Color trapColor = new(0.5f, 0.8f, 1.0f, 0.8f); // Light blue for traps

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor;
        }
    }

    public void SetOccupied(GameObject unit)
    {
        isOccupied = true;
        occupyingUnit = unit;
    }

    public void ClearOccupied()
    {
        isOccupied = false;
        occupyingUnit = null;
    }

    public void HighlightAsMovable()
    {
        if (spriteRenderer != null && isWalkable && !isOccupied)
        {
            spriteRenderer.color = highlightColor;
        }
    }

    public void HighlightAsAttackRange()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = attackRangeColor;
        }
    }

    public void ResetHighlight()
    {
        if (spriteRenderer != null)
        {
            if (!isWalkable)
            {
                spriteRenderer.color = blockedColor;
            }
            else if (trapObject != null)
            {
                spriteRenderer.color = trapColor;
            }
            else
            {
                spriteRenderer.color = defaultColor;
            }
        }
    }

    public void SetWalkable(bool walkable)
    {
        isWalkable = walkable;
        ResetHighlight();
    }

    void OnMouseEnter()
    {
        // Visual feedback when hovering over cell
        if (spriteRenderer != null && isWalkable && !isOccupied)
        {
            spriteRenderer.color = highlightColor * 1.2f;
        }
    }

    void OnMouseExit()
    {
        ResetHighlight();
    }

    public bool HasPowerUp()
    {
        return powerUpObject != null;
    }

    public void SetPowerUp(GameObject powerUp)
    {
        powerUpObject = powerUp;
    }

    public void ClearPowerUp()
    {
        powerUpObject = null;
    }

    public bool HasTrap()
    {
        return trapObject != null;
    }

    public void SetTrap(GameObject trap)
    {
        trapObject = trap;
        ResetHighlight(); // Update color to show trap
    }

    public void ClearTrap()
    {
        trapObject = null;
        ResetHighlight(); // Restore default color
    }
}