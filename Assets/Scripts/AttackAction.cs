using UnityEngine;

public class AttackAction : ActionBase
{
    public int damage = 1;

    public AttackAction() : base("Attack", 3, 2)
    {
    }

    public override bool CanExecute(GameObject actor, Vector2Int targetPosition)
    {
        if (!base.CanExecute(actor, targetPosition))
        {
            Debug.LogWarning("Attack: Base CanExecute failed (not enough power or wrong turn)");
            return false;
        }

        // Check if within range
        var actorController = actor.GetComponent<PlayerController>();
        Vector2Int actorPos = actorController != null ? actorController.GridPosition : actor.GetComponent<EnemyController>().GridPosition;

        int distance = Mathf.Abs(actorPos.x - targetPosition.x) +
                      Mathf.Abs(actorPos.y - targetPosition.y);

        if (distance > range || distance == 0)
        {
            Debug.LogWarning($"Attack: Target {targetPosition} out of range (distance={distance}, max range={range}) from {actorPos}");
            return false;
        }

        // Check if there's a target to attack
        GridCell targetCell = GridManager.Instance.GetCell(targetPosition);
        if (targetCell == null || !targetCell.isOccupied)
        {
            Debug.LogWarning($"Attack: Cell {targetPosition} is null or not occupied");
            return false;
        }

        // Check if target is an enemy (for player) or player (for enemy)
        GameObject target = targetCell.occupyingUnit;
        bool isPlayerAttacking = actorController != null;

        if (isPlayerAttacking)
        {
            // Player can only attack enemies
            bool hasEnemy = target.GetComponent<EnemyController>() != null;
            if (!hasEnemy)
            {
                Debug.LogWarning($"Attack: Cell {targetPosition} occupied by {target.name} but it's not an enemy");
            }
            return hasEnemy;
        }
        else
        {
            // Enemy can only attack player
            return target.GetComponent<PlayerController>() != null;
        }
    }

    public override void Execute(GameObject actor, Vector2Int targetPosition)
    {
        GridCell targetCell = GridManager.Instance.GetCell(targetPosition);
        if (targetCell != null && targetCell.isOccupied)
        {
            GameObject target = targetCell.occupyingUnit;

            // Get attacker position
            Vector2Int actorPos = GetActorPosition(actor);

            // Determine base damage amount
            int damageAmount = damage;
            var enemyController = actor.GetComponent<EnemyController>();
            if (enemyController != null && enemyController.enemyData != null)
            {
                damageAmount = enemyController.enemyData.attackDamage;
            }

            // Check for flanking/backstab bonuses (only for player attacking enemies)
            var playerController = actor.GetComponent<PlayerController>();
            if (playerController != null && target.GetComponent<EnemyController>() != null)
            {
                int positionBonus = CalculatePositionBonus(actorPos, targetPosition);
                damageAmount += positionBonus;

                if (positionBonus > 0)
                {
                    string bonusType = positionBonus >= 2 ? "BACKSTAB" : "FLANK";
                    Debug.Log($"{actor.name} {bonusType} bonus! +{positionBonus} damage");
                }
            }

            // Check for stun bonus - stunned enemies take double damage
            var enemyHealth = target.GetComponent<EnemyController>();
            if (enemyHealth != null)
            {
                StatusEffectManager statusManager = enemyHealth.GetStatusEffectManager();
                if (statusManager != null && statusManager.IsStunned())
                {
                    damageAmount *= 2;
                    Debug.Log($"{target.name} is STUNNED! Damage doubled to {damageAmount}!");
                }
            }

            // Deal damage
            var playerHealth = target.GetComponent<PlayerController>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
                Debug.Log($"{actor.name} attacked {target.name} for {damageAmount} damage!");
            }
            else if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log($"{actor.name} attacked {target.name} for {damageAmount} damage!");
            }

            // Consume power (only for player)
            if (playerController != null)
            {
                PowerManager.Instance.ConsumePower(powerCost);
            }
        }
    }

    private Vector2Int GetActorPosition(GameObject actor)
    {
        var playerController = actor.GetComponent<PlayerController>();
        if (playerController != null)
        {
            return playerController.GridPosition;
        }
        var enemyController = actor.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            return enemyController.GridPosition;
        }
        return Vector2Int.zero;
    }

    private int CalculatePositionBonus(Vector2Int attackerPos, Vector2Int targetPos)
    {
        // Get the target enemy's telegraphed action to determine facing
        GridCell targetCell = GridManager.Instance.GetCell(targetPos);
        if (targetCell == null || !targetCell.isOccupied)
        {
            return 0;
        }

        GameObject targetUnit = targetCell.occupyingUnit;
        EnemyController targetEnemy = targetUnit.GetComponent<EnemyController>();
        if (targetEnemy == null)
        {
            return 0; // Not an enemy
        }

        // Direction from target to attacker
        Vector2Int attackDirection = attackerPos - targetPos;
        Vector2Int normalizedAttackDir = new Vector2Int(
            attackDirection.x != 0 ? attackDirection.x / Mathf.Abs(attackDirection.x) : 0,
            attackDirection.y != 0 ? attackDirection.y / Mathf.Abs(attackDirection.y) : 0
        );

        // Check for backstab: attacking from opposite direction of enemy's telegraph
        if (targetEnemy.nextAction != null && targetEnemy.nextAction.IsValid())
        {
            // Direction from enemy to their target
            Vector2Int enemyTargetDirection = targetEnemy.nextAction.targetPosition - targetPos;
            Vector2Int normalizedEnemyDir = new Vector2Int(
                enemyTargetDirection.x != 0 ? enemyTargetDirection.x / Mathf.Abs(enemyTargetDirection.x) : 0,
                enemyTargetDirection.y != 0 ? enemyTargetDirection.y / Mathf.Abs(enemyTargetDirection.y) : 0
            );

            // If attacking from exactly opposite direction, it's a backstab
            if (normalizedAttackDir.x == -normalizedEnemyDir.x && normalizedAttackDir.y == -normalizedEnemyDir.y)
            {
                return 2; // BACKSTAB bonus
            }

            // If attacking from perpendicular direction (flanking from the side)
            // Check if the dot product is 0 (perpendicular vectors)
            int dotProduct = normalizedAttackDir.x * normalizedEnemyDir.x + normalizedAttackDir.y * normalizedEnemyDir.y;
            if (dotProduct == 0)
            {
                return 1; // FLANKING bonus
            }
        }

        // Check for flanking: enemy is cornered (3+ adjacent cells blocked)
        int blockedSides = 0;
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int adjacentPos = targetPos + dir;
            GridCell adjacentCell = GridManager.Instance.GetCell(adjacentPos);
            if (adjacentCell == null || !adjacentCell.isWalkable || adjacentCell.isOccupied)
            {
                blockedSides++;
            }
        }

        if (blockedSides >= 3)
        {
            return 1; // Enemy is cornered - flanking bonus
        }

        return 0; // No bonus
    }

    public override void ShowRange(Vector2Int fromPosition)
    {
        if (GridManager.Instance != null)
        {
            var cells = GridManager.Instance.GetCellsInRange(fromPosition, range, false);
            GridManager.Instance.HighlightCells(cells, true); // Red for attack
        }
    }
}