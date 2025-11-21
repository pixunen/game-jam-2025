using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

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
        // Subscribe to events
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerChanged += UpdatePowerDisplay;
            PowerManager.Instance.OnBonusPowerChanged += UpdateBonusPowerDisplay;
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnStateChanged += UpdateTurnDisplay;
        }

        // Hide game over panel initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Initial UI update
        UpdatePowerDisplay(PowerManager.Instance != null ? PowerManager.Instance.GetCurrentPower() : 0,
                          PowerManager.Instance != null ? PowerManager.Instance.GetMaxPower() : 10);
        UpdateTurnDisplay(TurnState.PlayerTurn);
    }

    void Update()
    {
        // Update player health display
        PlayerController player = GameObject.FindFirstObjectByType<PlayerController>();
        if (player != null && healthText != null)
        {
            healthText.text = $"Health: {player.currentHealth}/{player.maxHealth}";
        }

        // Check for restart input during game over
        if (TurnManager.Instance != null && TurnManager.Instance.currentTurnState == TurnState.GameOver)
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Debug.Log("R key pressed - restarting game!");
                RestartGame();
            }
        }
    }

    void RestartGame()
    {
        Debug.Log("RestartGame called in UIManager");
        if (GameManager.Instance != null)
        {
            Debug.Log("GameManager found, calling RestartGame");
            GameManager.Instance.RestartGame();
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }
    }

    void UpdatePowerDisplay(int current, int max)
    {
        if (powerText != null)
        {
            int bonusPower = PowerManager.Instance != null ? PowerManager.Instance.GetBonusPower() : 0;
            if (bonusPower > 0)
            {
                powerText.text = $"Power: {current}/{max} (+{bonusPower} bonus)";
            }
            else
            {
                powerText.text = $"Power: {current}/{max}";
            }
        }
    }

    void UpdateBonusPowerDisplay(int bonusPower)
    {
        // Trigger a full power display update to show the new bonus
        if (PowerManager.Instance != null)
        {
            UpdatePowerDisplay(PowerManager.Instance.GetCurrentPower(), PowerManager.Instance.GetMaxPower());
        }
    }

    void UpdateTurnDisplay(TurnState state)
    {
        if (turnText != null)
        {
            if (state == TurnState.PlayerTurn)
            {
                turnText.text = "PLAYER TURN";
                turnText.color = Color.green;
            }
            else if (state == TurnState.EnemyTurn)
            {
                turnText.text = "ENEMY TURN";
                turnText.color = Color.red;
            }
            else if (state == TurnState.GameOver)
            {
                turnText.text = "GAME OVER";
                turnText.color = Color.gray;
                ShowGameOver("Game Over!");
            }
        }
    }

    public void ShowGameOver(string message)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        if (gameOverText != null)
        {
            gameOverText.text = message + "\n\nPress R to Restart";
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.OnPowerChanged -= UpdatePowerDisplay;
            PowerManager.Instance.OnBonusPowerChanged -= UpdateBonusPowerDisplay;
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnStateChanged -= UpdateTurnDisplay;
        }
    }
}