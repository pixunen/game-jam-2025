using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Transform playerSpawnPoint;
    public Transform[] enemySpawnPoints;

    [Header("Game State")]
    public bool gameStarted = false;
    private GameObject playerInstance;
    public int score = 0;

    [Header("UI")]
    public UIManager uiManager;

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
        StartGame();
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
        }
    }


    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        Debug.Log("Game Started!");

        // Spawn player if prefab is assigned
        if (playerPrefab != null && playerSpawnPoint != null)
        {
            playerInstance = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        }

        // Spawn enemies if prefabs are assigned
        if (enemyPrefab != null && enemySpawnPoints != null)
        {
            foreach (Transform spawnPoint in enemySpawnPoints)
            {
                if (spawnPoint != null)
                {
                    Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                }
            }
        }
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}