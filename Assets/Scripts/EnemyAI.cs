using System.Collections.Generic;
using UnityEngine;

public class EnemyAI
{
    private EnemyController enemy;
    private int attackRange = 2;
    private int moveRange = 3;

    public EnemyAI(EnemyController controller)
    {
        enemy = controller;
    }

    public void SetRanges(int newMoveRange, int newAttackRange)
    {
        moveRange = newMoveRange;
        attackRange = newAttackRange;
    }

    public void DecideAction()
    {
        // Find player
        PlayerController player = FindPlayer();
        if (player == null)
        {
            Debug.LogWarning($"{enemy.gameObject.name}: No player found!");
            return;
        }

        Vector2Int playerPos = player.GridPosition;
        Vector2Int enemyPos = enemy.GridPosition;

        // Calculate distance to player
        int distance = Mathf.Abs(playerPos.x - enemyPos.x) + Mathf.Abs(playerPos.y - enemyPos.y);

        // If player is in attack range, attack
        if (distance <= attackRange && distance > 0)
        {
            Debug.Log($"{enemy.gameObject.name}: Attacking player!");
            enemy.PerformAttack(playerPos);
        }
        // Otherwise, move closer to player
        else
        {
            Vector2Int targetPos = GetMoveTowardsPlayer(playerPos, enemyPos);
            if (targetPos != enemyPos)
            {
                Debug.Log($"{enemy.gameObject.name}: Moving towards player to {targetPos}");
                enemy.ExecuteMove(targetPos);
            }
            else
            {
                Debug.Log($"{enemy.gameObject.name}: Cannot move closer to player.");
            }
        }
    }

    private PlayerController FindPlayer()
    {
        return GameObject.FindFirstObjectByType<PlayerController>();
    }

    private Vector2Int GetMoveTowardsPlayer(Vector2Int playerPos, Vector2Int enemyPos)
    {
        // Get all cells within move range
        List<GridCell> movableCells = GridManager.Instance.GetCellsInRange(enemyPos, moveRange, true);

        Vector2Int bestPosition = enemyPos;
        int shortestDistance = int.MaxValue;

        // Find the cell that gets us closest to the player
        foreach (GridCell cell in movableCells)
        {
            int distance = Mathf.Abs(playerPos.x - cell.gridPosition.x) +
                          Mathf.Abs(playerPos.y - cell.gridPosition.y);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                bestPosition = cell.gridPosition;
            }
        }

        return bestPosition;
    }
}