using UnityEngine;
using UnityEditor;
using System.IO;

public class EnemyDataCreator : MonoBehaviour
{
    [MenuItem("Tools/Create Enemy Data Assets")]
    public static void CreateEnemyDataAssets()
    {
        string folderPath = "Assets/ScriptableObjects/Enemies";

        // Ensure directory exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Create Basic Enemy
        CreateEnemyAsset(
            folderPath + "/BasicEnemy.asset",
            "Basic Enemy",
            maxHealth: 2,
            attackDamage: 1,
            moveRange: 3,
            attackRange: 2,
            spawnWeight: 10,
            minWave: 1,
            color: new Color(1f, 0.2f, 0.2f, 1f)
        );

        // Create Fast Enemy
        CreateEnemyAsset(
            folderPath + "/FastEnemy.asset",
            "Fast Enemy",
            maxHealth: 1,
            attackDamage: 1,
            moveRange: 4,
            attackRange: 2,
            spawnWeight: 8,
            minWave: 2,
            color: new Color(1f, 0.8f, 0.2f, 1f)
        );

        // Create Tank Enemy
        CreateEnemyAsset(
            folderPath + "/TankEnemy.asset",
            "Tank Enemy",
            maxHealth: 5,
            attackDamage: 2,
            moveRange: 2,
            attackRange: 2,
            spawnWeight: 5,
            minWave: 3,
            color: new Color(0.5f, 0.2f, 0.8f, 1f)
        );

        // Create Aggressive Enemy
        CreateEnemyAsset(
            folderPath + "/AggressiveEnemy.asset",
            "Aggressive Enemy",
            maxHealth: 3,
            attackDamage: 2,
            moveRange: 3,
            attackRange: 3,
            spawnWeight: 6,
            minWave: 4,
            color: new Color(0.9f, 0.1f, 0.1f, 1f)
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Enemy Data assets created successfully!");
    }

    private static void CreateEnemyAsset(string path, string enemyName, int maxHealth, int attackDamage,
        int moveRange, int attackRange, int spawnWeight, int minWave, Color color)
    {
        EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
        data.enemyName = enemyName;
        data.maxHealth = maxHealth;
        data.attackDamage = attackDamage;
        data.moveRange = moveRange;
        data.attackRange = attackRange;
        data.spawnWeight = spawnWeight;
        data.minWaveToSpawn = minWave;
        data.enemyColor = color;

        AssetDatabase.CreateAsset(data, path);
        Debug.Log($"Created {enemyName} at {path}");
    }
}
