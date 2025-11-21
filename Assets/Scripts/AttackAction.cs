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
        Vector2Int actorPos = actorController != null ? actorController.gridPosition : actor.GetComponent<EnemyController>().gridPosition;

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

            // Determine damage amount
            int damageAmount = damage;
            var enemyController = actor.GetComponent<EnemyController>();
            if (enemyController != null && enemyController.enemyData != null)
            {
                damageAmount = enemyController.enemyData.attackDamage;
            }

            // Deal damage
            var enemyHealth = target.GetComponent<EnemyController>();
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
            var playerController = actor.GetComponent<PlayerController>();
            if (playerController != null)
            {
                PowerManager.Instance.ConsumePower(powerCost);
            }
        }
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