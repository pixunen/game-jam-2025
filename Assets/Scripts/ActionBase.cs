using UnityEngine;

public abstract class ActionBase
{
    public string actionName;
    public int powerCost;
    public int range;

    public ActionBase(string name, int cost, int actionRange)
    {
        actionName = name;
        powerCost = cost;
        range = actionRange;
    }

    public virtual bool CanExecute(GameObject actor, Vector2Int targetPosition)
    {
        // Check if it's player's turn (for player actions)
        PlayerController playerController = actor.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Check if actor has enough power (only for player)
            if (!PowerManager.Instance.HasEnoughPower(powerCost))
            {
                return false;
            }

            // Check if it's player's turn
            if (!TurnManager.Instance.IsPlayerTurn())
            {
                return false;
            }
        }

        return true;
    }

    public abstract void Execute(GameObject actor, Vector2Int targetPosition);

    public virtual void ShowRange(Vector2Int fromPosition)
    {
        if (GridManager.Instance != null)
        {
            var cells = GridManager.Instance.GetCellsInRange(fromPosition, range, false);
            GridManager.Instance.HighlightCells(cells, false);
        }
    }
}