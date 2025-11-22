using UnityEngine;

public class IceTrapAction : ActionBase
{
    public IceTrapAction() : base("Ice Trap", 3, 4)
    {
    }

    public override bool CanExecute(GameObject actor, Vector2Int targetPosition)
    {
        if (!base.CanExecute(actor, targetPosition))
        {
            Debug.LogWarning("IceTrap: Base CanExecute failed (not enough power or wrong turn)");
            return false;
        }

        // Check if it's the player
        var actorController = actor.GetComponent<PlayerController>();
        if (actorController == null)
        {
            Debug.LogWarning("IceTrap: Can only be used by player");
            return false;
        }

        Vector2Int actorPos = actorController.GridPosition;
        int distance = Mathf.Abs(actorPos.x - targetPosition.x) +
                      Mathf.Abs(actorPos.y - targetPosition.y);

        if (distance > range || distance == 0)
        {
            Debug.LogWarning($"IceTrap: Target {targetPosition} out of range (distance={distance}, max range={range}) from {actorPos}");
            return false;
        }

        // Check if target cell is valid and empty
        GridCell targetCell = GridManager.Instance.GetCell(targetPosition);
        if (targetCell == null)
        {
            Debug.LogWarning($"IceTrap: Cell {targetPosition} is null");
            return false;
        }

        if (!targetCell.isWalkable)
        {
            Debug.LogWarning($"IceTrap: Cell {targetPosition} is not walkable");
            return false;
        }

        if (targetCell.isOccupied)
        {
            Debug.LogWarning($"IceTrap: Cell {targetPosition} is occupied");
            return false;
        }

        // Check if there's already a trap at this location
        IceTrap[] existingTraps = GameObject.FindObjectsByType<IceTrap>(FindObjectsSortMode.None);
        foreach (IceTrap trap in existingTraps)
        {
            if (trap.GridPosition == targetPosition && !trap.IsTriggered())
            {
                Debug.LogWarning($"IceTrap: There's already a trap at {targetPosition}");
                return false;
            }
        }

        return true;
    }

    public override void Execute(GameObject actor, Vector2Int targetPosition)
    {
        // Create trap GameObject
        GameObject trapObject = new GameObject("IceTrap");
        IceTrap trap = trapObject.AddComponent<IceTrap>();
        trap.Initialize(targetPosition);

        // Consume power
        var playerController = actor.GetComponent<PlayerController>();
        if (playerController != null)
        {
            PowerManager.Instance.ConsumePower(powerCost);
        }

        Debug.Log($"{actor.name} placed an Ice Trap at {targetPosition}!");
    }

    public override void ShowRange(Vector2Int fromPosition)
    {
        if (GridManager.Instance != null)
        {
            var cells = GridManager.Instance.GetCellsInRange(fromPosition, range, true);
            GridManager.Instance.HighlightCells(cells, false); // Blue for CC
        }
    }
}