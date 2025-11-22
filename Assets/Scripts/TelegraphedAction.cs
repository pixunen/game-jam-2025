using UnityEngine;

public enum TelegraphActionType
{
    None,
    Move,
    Attack,
    Special
}

[System.Serializable]
public class TelegraphedAction
{
    public TelegraphActionType actionType;
    public Vector2Int targetPosition;
    public Vector2Int executingFromPosition; // Enemy position when action was planned
    
    public TelegraphedAction(TelegraphActionType type, Vector2Int target, Vector2Int fromPos)
    {
        actionType = type;
        targetPosition = target;
        executingFromPosition = fromPos;
    }
    
    public bool IsValid()
    {
        return actionType != TelegraphActionType.None;
    }
}