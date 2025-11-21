using UnityEngine;
using TMPro;

public class KeybindingDisplay : MonoBehaviour
{
    void Start()
    {
        CreateKeybindingText();
    }

    void CreateKeybindingText()
    {
        // Create a new GameObject for the text
        GameObject textObj = new GameObject("KeybindingText");
        textObj.transform.SetParent(transform, false);

        // Add TextMeshPro component
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "[M] Move (2 Power)  |  [A] Attack (3 Power)  |  [S] Special (5 Power)  |  [E] End Turn  |  [C] Cancel";
        tmp.fontSize = 16;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        // Position at bottom of screen
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0, 10);
        rect.sizeDelta = new Vector2(800, 30);

        Debug.Log("Keybinding display created!");
    }
}