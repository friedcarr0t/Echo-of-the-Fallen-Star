using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menangani UI "Game Finished" di chapter terakhir.
/// - Pasang script ini di GameObject di Scene 3 (chapter boss).
/// - Assign panel UI "Game Finished" ke field gameFinishedPanel.
/// - Panel akan diaktifkan ketika StarManager memberi sinyal bahwa star chapter 3 sudah diambil.
/// </summary>
public class GameFinishedUIManager : MonoBehaviour
{
    public static GameFinishedUIManager Instance;

    [Header("Game Finished UI")]
    public GameObject gameFinishedPanel;

    private bool isShown = false;

    void Awake()
    {
        // Per-scene singleton (mirip GameOverManager)
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }

    void Start()
    {
        if (gameFinishedPanel != null)
        {
            gameFinishedPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[GameFinishedUIManager] gameFinishedPanel TIDAK DI-ASSIGN di Inspector!");
        }
    }

    public void ShowGameFinished()
    {
        if (isShown) return;
        isShown = true;

        Time.timeScale = 0f; // pause game

        if (gameFinishedPanel != null)
        {
            gameFinishedPanel.SetActive(true);
            Debug.Log("[GameFinishedUIManager] Panel Game Finished diaktifkan.");
        }
        else
        {
            Debug.LogError("[GameFinishedUIManager] gameFinishedPanel null saat ShowGameFinished dipanggil!");
        }
    }

    // Dipanggil dari tombol "Restart" di panel Game Finished (opsional)
    public void RestartFromBeginning()
    {
        Time.timeScale = 1f;
        // Asumsikan chapter 1 adalah buildIndex 0
        SceneManager.LoadScene(0);
    }

    // Dipanggil dari tombol "Quit" (opsional)
    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}


