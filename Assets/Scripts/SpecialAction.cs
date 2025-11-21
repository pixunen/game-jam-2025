using UnityEngine;

public class SpecialAction : ActionBase
{
    public int damage = 2;

    public SpecialAction() : base("Special", 5, 3)
    {
    }

    public override bool CanExecute(GameObject actor, Vector2Int targetPosition)
    {
        if (!base.CanExecute(actor, targetPosition))
        {
            return false;
        }

        // Check if within range
        var actorController = actor.GetComponent<PlayerController>();
        Vector2Int actorPos = actorController != null ? actorController.GridPosition : actor.GetComponent<EnemyController>().GridPosition;

        int distance = Mathf.Abs(actorPos.x - targetPosition.x) +
                      Mathf.Abs(actorPos.y - targetPosition.y);

        return distance <= range && distance > 0;
    }

    public override void Execute(GameObject actor, Vector2Int targetPosition)
    {
        Debug.Log($"{actor.name} used Special Ability on {targetPosition}!");

        // Area of effect damage
        var cellsInArea = GridManager.Instance.GetCellsInRange(targetPosition, 1, false);

        foreach (var cell in cellsInArea)
        {
            if (cell.isOccupied && cell.occupyingUnit != null)
            {
                var enemy = cell.occupyingUnit.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"Special hit {enemy.name} for {damage} damage!");
                }
            }
        }

        // Consume power (only for player)
        var playerController = actor.GetComponent<PlayerController>();
        if (playerController != null)
        {
            PowerManager.Instance.ConsumePower(powerCost);
        }

        // Visual feedback could be added here (particle effects, etc.)
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