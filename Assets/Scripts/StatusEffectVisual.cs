using TMPro;
using UnityEngine;

public class StatusEffectVisual : MonoBehaviour
{
    private TextMeshPro statusText;
    private StatusEffectManager statusEffectManager;
    private Vector3 offset = new Vector3(0, 0.8f, 0); // Above the unit (top position)

    void Start()
    {
        // Get status effect manager
        statusEffectManager = GetComponent<StatusEffectManager>();

        if (statusEffectManager == null)
        {
            // Try to add one if it doesn't exist
            statusEffectManager = gameObject.AddComponent<StatusEffectManager>();
        }

        // Create text object for status display
        GameObject textObj = new GameObject("StatusText");
        textObj.transform.parent = transform;
        textObj.transform.localPosition = offset;

        statusText = textObj.AddComponent<TextMeshPro>();
        statusText.fontSize = 3;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.sortingOrder = 102; // On top of everything (above telegraph)

        // Start hidden
        statusText.text = "";
    }

    void Update()
    {
        if (statusEffectManager == null || statusText == null) return;

        // Update status text
        if (statusEffectManager.HasStatusEffect())
        {
            StatusEffect effect = statusEffectManager.GetCurrentEffect();
            string statusIcon = GetStatusIcon(effect.type);
            string durationText = effect.duration > 1 ? $" ({effect.duration})" : "";

            statusText.text = statusIcon + durationText;
            statusText.color = GetStatusColor(effect.type);
        }
        else
        {
            statusText.text = "";
        }
    }

    private string GetStatusIcon(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Stunned:
                return "⚡"; // ⚡ High Voltage (U+26A1)
            case StatusEffectType.Rooted:
                return "❄️"; // ❄ Snowflake (U+2744)
            case StatusEffectType.Slowed:
                return "🔽"; // ▼ Down Triangle (U+25BC)
            default:
                return "?";
        }
    }

    private Color GetStatusColor(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Stunned:
                return new Color(1f, 1f, 0.3f); // Yellow
            case StatusEffectType.Rooted:
                return new Color(0.5f, 0.8f, 1f); // Light blue
            case StatusEffectType.Slowed:
                return new Color(0.8f, 0.8f, 0.8f); // Gray
            default:
                return Color.white;
        }
    }
}