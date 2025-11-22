using TMPro;
using UnityEngine;

public class EnemyTelegraphIndicator : MonoBehaviour
{
    private TextMeshPro telegraphText;
    private EnemyController enemyController;
    private Vector3 offset = new Vector3(0, 0.6f, 0); // Above the unit (below status effect)

    void Start()
    {
        // Get enemy controller
        enemyController = GetComponent<EnemyController>();

        if (enemyController == null)
        {
            Debug.LogWarning("EnemyTelegraphIndicator: No EnemyController found!");
            return;
        }

        // Create text object for telegraph display
        GameObject textObj = new GameObject("TelegraphIndicator");
        textObj.transform.parent = transform;
        textObj.transform.localPosition = offset;

        telegraphText = textObj.AddComponent<TextMeshPro>();
        telegraphText.fontSize = 4;
        telegraphText.alignment = TextAlignmentOptions.Center;
        telegraphText.sortingOrder = 101; // Above status effects

        // Start hidden
        telegraphText.text = "";
    }

    void Update()
    {
        if (enemyController == null || telegraphText == null) return;

        // Update telegraph indicator based on next action
        if (enemyController.nextAction != null && enemyController.nextAction.IsValid())
        {
            TelegraphedAction action = enemyController.nextAction;

            switch (action.actionType)
            {
                case TelegraphActionType.Attack:
                    telegraphText.text = "⚔️";
                    telegraphText.color = new Color(1f, 0.3f, 0.3f); // Red
                    break;
                case TelegraphActionType.Move:
                    telegraphText.text = "➡️";
                    telegraphText.color = new Color(1f, 1f, 0.3f); // Yellow
                    break;
                default:
                    telegraphText.text = "";
                    break;
            }
        }
        else
        {
            telegraphText.text = "";
        }
    }
}