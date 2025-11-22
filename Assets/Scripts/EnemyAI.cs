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

    // Legacy method - kept for backward compatibility but now calls DecideNextAction
    public void DecideAction()
    {
        TelegraphedAction action = DecideNextAction();
        if (action != null && action.IsValid())
        {
            // Execute immediately (for legacy code paths)
            switch (action.actionType)
            {
                case TelegraphActionType.Attack:
                    enemy.PerformAttack(action.targetPosition);
                    break;
                case TelegraphActionType.Move:
                    enemy.ExecuteMove(action.targetPosition);
                    break;
            }
        }
    }

    // New method that returns a telegraphed action instead of executing
    public TelegraphedAction DecideNextAction()
    {
        // Find player
        PlayerController player = FindPlayer();
        if (player == null)
        {
            Debug.LogWarning($"{enemy.gameObject.name}: No player found!");
            return new TelegraphedAction(TelegraphActionType.None, Vector2Int.zero, enemy.GridPosition);
        }

        Vector2Int playerPos = player.GridPosition;
        Vector2Int enemyPos = enemy.GridPosition;

        // Calculate distance to player
        int distance = Mathf.Abs(playerPos.x - enemyPos.x) + Mathf.Abs(playerPos.y - enemyPos.y);

        // If player is in attack range, telegraph attack
        if (distance <= attackRange && distance > 0)
        {
            Debug.Log($"{enemy.gameObject.name}: Telegraphing attack at {playerPos}");
            return new TelegraphedAction(TelegraphActionType.Attack, playerPos, enemyPos);
        }
        // Otherwise, telegraph move closer to player
        else
        {
            Vector2Int targetPos = GetMoveTowardsPlayer(playerPos, enemyPos);
            if (targetPos != enemyPos)
            {
                Debug.Log($"{enemy.gameObject.name}: Telegraphing move to {targetPos}");
                return new TelegraphedAction(TelegraphActionType.Move, targetPos, enemyPos);
            }
            else
            {
                Debug.Log($"{enemy.gameObject.name}: Cannot move closer to player - no action telegraphed.");
                return new TelegraphedAction(TelegraphActionType.None, enemyPos, enemyPos);
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