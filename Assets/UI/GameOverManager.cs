using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    
    [Header("Settings")]
    public float deathAnimationDelay = 2f;  // Delay untuk animasi death player (tidak dipakai jika instantGameOverOnDeath = true)
    public float fallDelay = 0.5f;          // Delay untuk jatuh (lebih cepat)
    [Tooltip("Jika true, ketika HP player 0, panel game over langsung muncul tanpa jeda.")]
    public bool instantGameOverOnDeath = true;

    private bool isGameOver = false;

    void Awake()
    {
        // Selalu pastikan hanya ada satu Instance per scene.
        // Jika ada sisa Instance dari scene sebelumnya, hancurkan yang lama
        // dan gunakan yang baru (per-scene manager, bukan global lagi).
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver || gameOverPanel == null)
            return;
        
        isGameOver = true;

        if (instantGameOverOnDeath)
        {
            ShowGameOverImmediate();
        }
        else
        {
            StartCoroutine(ShowGameOverAfterDelay(Mathf.Max(0f, deathAnimationDelay)));
        }
    }

    // Dipanggil ketika player jatuh
    public void TriggerGameOverFall()
    {
        if (isGameOver) return;
        isGameOver = true;

        StartCoroutine(ShowGameOverAfterDelay(fallDelay));
    }

    private void ShowGameOverImmediate()
    {
        Time.timeScale = 0f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    private System.Collections.IEnumerator ShowGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Time.timeScale = 0f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // Dipanggil dari button Restart
    public void RestartChapter()
    {
        Time.timeScale = 1f; // Unpause
        isGameOver = false;
        
        // Reload scene yang sedang aktif (chapter saat ini)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Dipanggil dari button Main Menu (optional)
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene("MainMenu"); // Ganti dengan nama scene menu kamu
    }
}

