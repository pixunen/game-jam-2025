using UnityEngine;

[CreateAssetMenu(fileName = "PowerUp", menuName = "ScriptableObjects/PowerUpData", order = 2)]
public class PowerUpData : ScriptableObject
{
    [Header("Power Settings")]
    [Tooltip("Minimum power amount this power-up can give")]
    public int minPowerAmount = 2;
    
    [Tooltip("Maximum power amount this power-up can give")]
    public int maxPowerAmount = 5;
    
    [Header("Spawn Settings")]
    [Tooltip("Number of turns before this power-up despawns if not collected")]
    public int despawnTurns = 3;
    
    [Header("Visual Settings")]
    [Tooltip("Sprite to display for this power-up")]
    public Sprite powerUpSprite;
    
    [Tooltip("Color tint for the power-up")]
    public Color powerUpColor = Color.yellow;
    
    /// <summary>
    /// Get a random power amount within the configured range
    /// </summary>
    public int GetRandomPowerAmount()
    {
        return Random.Range(minPowerAmount, maxPowerAmount + 1);
    }
}