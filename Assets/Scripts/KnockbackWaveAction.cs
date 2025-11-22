using UnityEngine;
using System.Collections.Generic;

public class KnockbackWaveAction : ActionBase
{
    public int knockbackDistance = 2;

    public KnockbackWaveAction() : base("Knockback Wave", 5, 1) // Range 1 means adjacent only
    {
    }

    public override bool CanExecute(GameObject actor, Vector2Int targetPosition)
    {
        if (!base.CanExecute(actor, targetPosition))
        {
            Debug.LogWarning("KnockbackWave: Base CanExecute failed (not enough power or wrong turn)");
            return false;
        }

        // Check if it's the player
        var actorController = actor.GetComponent<PlayerController>();
        if (actorController == null)
        {
            Debug.LogWarning("KnockbackWave: Can only be used by player");
            return false;
        }

        // This ability targets the player's own position (affects all adjacent)
        Vector2Int actorPos = actorController.GridPosition;
        if (targetPosition != actorPos)
        {
            Debug.LogWarning("KnockbackWave: Must target your own position (affects adjacent enemies)");
            return false;
        }

        // Check if there's at least one adjacent enemy
        bool hasAdjacentEnemy = false;
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = actorPos + dir;
            GridCell cell = GridManager.Instance.GetCell(checkPos);
            if (cell != null && cell.isOccupied)
            {
                if (cell.occupyingUnit.GetComponent<EnemyController>() != null)
                {
                    hasAdjacentEnemy = true;
                    break;
                }
            }
        }

        if (!hasAdjacentEnemy)
        {
            Debug.LogWarning("KnockbackWave: No adjacent enemies to knockback");
        }

        return hasAdjacentEnemy;
    }

    public override void Execute(GameObject actor, Vector2Int targetPosition)
    {
        var playerController = actor.GetComponent<PlayerController>();
        if (playerController == null) return;

        Vector2Int actorPos = playerController.GridPosition;

        // Find all adjacent enemies
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        List<GameObject> enemiesToKnockback = new List<GameObject>();
        List<Vector2Int> knockbackDirections = new List<Vector2Int>();

        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = actorPos + dir;
            GridCell cell = GridManager.Instance.GetCell(checkPos);
            if (cell != null && cell.isOccupied)
            {
                GameObject unit = cell.occupyingUnit;
                if (unit.GetComponent<EnemyController>() != null)
                {
                    enemiesToKnockback.Add(unit);
                    knockbackDirections.Add(dir);
                }
            }
        }

        // Knockback each enemy
        int knockbackCount = 0;
        for (int i = 0; i < enemiesToKnockback.Count; i++)
        {
            GameObject enemy = enemiesToKnockback[i];
            Vector2Int direction = knockbackDirections[i];

            if (KnockbackEnemy(enemy, direction))
            {
                knockbackCount++;
            }
        }

        Debug.Log($"{actor.name} used Knockback Wave! Pushed {knockbackCount} enemies back.");

        // Consume power
        if (playerController != null)
        {
            PowerManager.Instance.ConsumePower(powerCost);
        }
    }

    private bool KnockbackEnemy(GameObject enemy, Vector2Int direction)
    {
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController == null) return false;

        Vector2Int currentPos = enemyController.GridPosition;
        Vector2Int targetPos = currentPos;

        // Try to push enemy the full knockback distance
        for (int distance = knockbackDistance; distance > 0; distance--)
        {
            Vector2Int testPos = currentPos + (direction * distance);
            GridCell testCell = GridManager.Instance.GetCell(testPos);

            // Check if this position is valid
            if (testCell != null && testCell.isWalkable && !testCell.isOccupied)
            {
                targetPos = testPos;
                break;
            }
        }

        // If we found a valid position, move the enemy there
        if (targetPos != currentPos)
        {
            // Clear current cell
            GridCell currentCell = GridManager.Instance.GetCell(currentPos);
            if (currentCell != null)
            {
                currentCell.ClearOccupied();
            }

            // Move to new position
            GridCell newCell = GridManager.Instance.GetCell(targetPos);
            if (newCell != null)
            {
                Vector3 worldPos = GridManager.Instance.GetWorldPosition(targetPos);
                enemy.transform.position = worldPos;
                newCell.SetOccupied(enemy);

                int actualDistance = Mathf.Abs(targetPos.x - currentPos.x) + Mathf.Abs(targetPos.y - currentPos.y);
                Debug.Log($"Knocked back {enemy.name} by {actualDistance} cells from {currentPos} to {targetPos}");
                return true;
            }
        }

        Debug.Log($"Could not knockback {enemy.name} - no valid position found");
        return false;
    }

    public override void ShowRange(Vector2Int fromPosition)
    {
        if (GridManager.Instance != null)
        {
            // Highlight adjacent cells (range 1)
            var cells = GridManager.Instance.GetCellsInRange(fromPosition, range, false);
            GridManager.Instance.HighlightCells(cells, false); // Blue for CC
        }
    }
}