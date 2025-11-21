using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance { get; private set; }

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public List<EnemyData> availableEnemyTypes = new();

    [Header("Wave Settings")]
    public int currentWave = 1;
    public int turnsUntilNextSpawn = 3;
    private int turnsSinceLastSpawn = 0;

    [Header("Spawn Progression")]
    [Tooltip("Base enemies to spawn per wave")]
    public int baseSpawnCount = 1;
    [Tooltip("Additional enemies per wave level")]
    public float spawnScaling = 0.5f;
    [Tooltip("Turns between spawns decrease per wave")]
    public int minTurnsBetweenSpawns = 1;
    [Tooltip("Initial turns between spawns")]
    public int maxTurnsBetweenSpawns = 3;

    [Header("Wave Thresholds")]
    [Tooltip("Turns per wave (wave increases every X turns)")]
    public int turnsPerWave = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Subscribe to turn events
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart += OnPlayerTurnStart;
        }

        // Spawn initial wave
        SpawnInitialWave();
    }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerTurnStart -= OnPlayerTurnStart;
        }
    }

    void OnPlayerTurnStart()
    {
        // Update wave based on turn number
        UpdateWave();

        // Check if it's time to spawn
        turnsSinceLastSpawn++;
        if (turnsSinceLastSpawn >= turnsUntilNextSpawn)
        {
            SpawnWave();
            turnsSinceLastSpawn = 0;
            UpdateSpawnInterval();
        }
    }

    void UpdateWave()
    {
        int newWave = 1 + (TurnManager.Instance.turnNumber / turnsPerWave);
        if (newWave > currentWave)
        {
            currentWave = newWave;
            Debug.Log($"=== WAVE {currentWave} ===");
        }
    }

    void UpdateSpawnInterval()
    {
        // Decrease spawn interval as waves progress (more frequent spawns)
        turnsUntilNextSpawn = Mathf.Max(
            minTurnsBetweenSpawns,
            maxTurnsBetweenSpawns - (currentWave / 2)
        );
    }

    void SpawnInitialWave()
    {
        Debug.Log("Spawning initial wave...");
        int spawnCount = Mathf.Max(1, baseSpawnCount);
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnWave()
    {
        // Calculate how many enemies to spawn this wave
        int spawnCount = Mathf.RoundToInt(baseSpawnCount + (currentWave * spawnScaling));
        spawnCount = Mathf.Max(1, spawnCount); // At least 1 enemy

        Debug.Log($"Spawning wave: {spawnCount} enemies (Wave {currentWave})");

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || GridManager.Instance == null)
        {
            Debug.LogWarning("Cannot spawn enemy: missing prefab or GridManager");
            return;
        }

        // Get available enemy types for current wave
        List<EnemyData> spawnableEnemies = availableEnemyTypes
            .Where(e => e.minWaveToSpawn <= currentWave)
            .ToList();

        if (spawnableEnemies.Count == 0)
        {
            Debug.LogWarning("No spawnable enemy types available for current wave!");
            return;
        }

        // Select enemy type using weighted random
        EnemyData selectedEnemy = SelectWeightedRandom(spawnableEnemies);

        // Find random spawn position
        Vector2Int spawnPos = FindRandomSpawnPosition();
        if (spawnPos == Vector2Int.one * -1) // Failed to find position
        {
            Debug.LogWarning("Could not find valid spawn position!");
            return;
        }

        // Instantiate enemy
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(spawnPos);
        GameObject enemyObj = Instantiate(enemyPrefab, worldPos, Quaternion.identity);
        enemyObj.name = $"{selectedEnemy.enemyName}_{Time.frameCount}";

        // Initialize enemy with data
        EnemyController controller = enemyObj.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.Initialize(selectedEnemy, spawnPos);
        }

        Debug.Log($"Spawned {selectedEnemy.enemyName} at {spawnPos}");
    }

    EnemyData SelectWeightedRandom(List<EnemyData> enemies)
    {
        int totalWeight = enemies.Sum(e => e.spawnWeight);
        int randomValue = Random.Range(0, totalWeight);

        int currentWeight = 0;
        foreach (EnemyData enemy in enemies)
        {
            currentWeight += enemy.spawnWeight;
            if (randomValue < currentWeight)
            {
                return enemy;
            }
        }

        return enemies[0]; // Fallback
    }

    Vector2Int FindRandomSpawnPosition()
    {
        int attempts = 0;
        int maxAttempts = 100;

        while (attempts < maxAttempts)
        {
            Vector2Int randomPos = new(
                Random.Range(0, GridManager.Instance.gridWidth),
                Random.Range(0, GridManager.Instance.gridHeight)
            );

            GridCell cell = GridManager.Instance.GetCell(randomPos);
            if (cell != null && !cell.isOccupied && cell.isWalkable)
            {
                return randomPos;
            }

            attempts++;
        }

        Debug.LogError("Failed to find spawn position after 100 attempts!");
        return Vector2Int.one * -1; // Invalid position marker
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetTurnsUntilNextSpawn()
    {
        return turnsUntilNextSpawn - turnsSinceLastSpawn;
    }
}
