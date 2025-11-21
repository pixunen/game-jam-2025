using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Enemy";

    [Header("Stats")]
    public int maxHealth = 2;
    public int attackDamage = 1;
    public int score = 10;
    public int moveRange = 3;
    public int attackRange = 2;

    [Header("Spawn Settings")]
    [Tooltip("Higher weight = more likely to spawn")]
    public int spawnWeight = 10;
    [Tooltip("Minimum wave number to start spawning this enemy type")]
    public int minWaveToSpawn = 1;

    [Header("Visual")]
    public Color enemyColor = new(1f, 0.2f, 0.2f, 1f);
    public Sprite enemySprite;
}
