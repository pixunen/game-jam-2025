using UnityEngine;

public class StunBlastAction : ActionBase
{
    public int stunDuration = 1; // turns

    public StunBlastAction() : base("Stun Blast", 4, 3)
    {
    }

    public override bool CanExecute(GameObject actor, Vector2Int targetPosition)
    {
        if (!base.CanExecute(actor, targetPosition))
        {
            Debug.LogWarning("StunBlast: Base CanExecute failed (not enough power or wrong turn)");
            return false;
        }

        // Check if within range
        var actorController = actor.GetComponent<PlayerController>();
        if (actorController == null)
        {
            Debug.LogWarning("StunBlast: Can only be used by player");
            return false;
        }

        Vector2Int actorPos = actorController.GridPosition;
        int distance = Mathf.Abs(actorPos.x - targetPosition.x) +
                      Mathf.Abs(actorPos.y - targetPosition.y);

        if (distance > range || distance == 0)
        {
            Debug.LogWarning($"StunBlast: Target {targetPosition} out of range (distance={distance}, max range={range}) from {actorPos}");
            return false;
        }

        // Check if there's a target to stun
        GridCell targetCell = GridManager.Instance.GetCell(targetPosition);
        if (targetCell == null || !targetCell.isOccupied)
        {
            Debug.LogWarning($"StunBlast: Cell {targetPosition} is null or not occupied");
            return false;
        }

        // Check if target is an enemy
        GameObject target = targetCell.occupyingUnit;
        bool hasEnemy = target.GetComponent<EnemyController>() != null;
        if (!hasEnemy)
        {
            Debug.LogWarning($"StunBlast: Cell {targetPosition} occupied by {target.name} but it's not an enemy");
        }
        return hasEnemy;
    }

    public override void Execute(GameObject actor, Vector2Int targetPosition)
    {
        GridCell targetCell = GridManager.Instance.GetCell(targetPosition);
        if (targetCell != null && targetCell.isOccupied)
        {
            GameObject target = targetCell.occupyingUnit;
            EnemyController enemyController = target.GetComponent<EnemyController>();

            if (enemyController != null)
            {
                // Apply stun effect
                StatusEffect stunEffect = new StatusEffect(StatusEffectType.Stunned, stunDuration);
                enemyController.ApplyStatusEffect(stunEffect);
                Debug.Log($"{actor.name} stunned {target.name} for {stunDuration} turn(s)!");
            }

            // Consume power
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
            GridManager.Instance.HighlightCells(cells, false); // Blue for CC
        }
    }
}