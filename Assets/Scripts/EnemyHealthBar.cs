using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private EnemyController enemy;
    private GameObject healthBarCanvas;
    private Image healthBarFill;
    private Canvas canvas;

    void Start()
    {
        enemy = GetComponent<EnemyController>();
        CreateHealthBar();
    }

    void CreateHealthBar()
    {
        // Create canvas for health bar
        healthBarCanvas = new GameObject("HealthBarCanvas");
        healthBarCanvas.transform.SetParent(transform, false);
        healthBarCanvas.transform.localPosition = new Vector3(0, 0.6f, 0);

        canvas = healthBarCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = healthBarCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100f, 15f);
        canvasRect.localScale = new Vector3(0.008f, 0.008f, 1f);

        // Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarCanvas.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Health bar fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(healthBarCanvas.transform, false);
        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;
        fillRect.pivot = new Vector2(0, 0.5f);

        UpdateHealthBar();
    }

    void Update()
    {
        if (enemy != null)
        {
            UpdateHealthBar();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null && enemy != null)
        {
            float healthPercent = (float)enemy.currentHealth / enemy.maxHealth;
            healthBarFill.fillAmount = healthPercent;

            // Change color based on health
            if (healthPercent > 0.6f)
            {
                healthBarFill.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
            }
            else if (healthPercent > 0.3f)
            {
                healthBarFill.color = new Color(0.9f, 0.7f, 0.2f, 1f); // Yellow
            }
            else
            {
                healthBarFill.color = new Color(0.9f, 0.2f, 0.2f, 1f); // Red
            }
        }
    }
}