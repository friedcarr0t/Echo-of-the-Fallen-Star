using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class StarManager : MonoBehaviour
{
    public static StarManager Instance;

    // Event untuk notifikasi ketika star dikoleksi
    public static event Action<int> OnStarCollected;

    // Index 0 = chapter1, index 1 = chapter2, index 2 = chapter3
    public bool[] collectedStars = new bool[3];

    [Header("Scene Flow Settings")]
    [Tooltip("Jika true, saat star diambil akan otomatis pindah ke scene berikutnya.")]
    public bool autoLoadNextSceneOnCollect = false;
    [Tooltip("Delay sebelum pindah scene setelah star diambil.")]
    public float loadNextSceneDelay = 0.5f;
    [Tooltip("Nama scene tujuan untuk tiap chapter index. Contoh: [0] = Chapter2, [1] = Chapter3, [2] = FinishScene.")]
    public string[] nextSceneNames = new string[3];

    [Header("Game Finished UI")]
    [Tooltip("Jika true, ketika star chapter terakhir (index tertinggi) diambil, akan memunculkan GameFinishedUIManager daripada pindah scene.")]
    public bool useGameFinishedUIForLastChapter = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var activeScene = SceneManager.GetActiveScene();

            if (activeScene.buildIndex == 0)
            {
                for (int i = 0; i < collectedStars.Length; i++)
                {
                    PlayerPrefs.DeleteKey($"StarCollected_{i}");
                    collectedStars[i] = false;
                }
                PlayerPrefs.Save();
            }

            for (int i = 0; i < collectedStars.Length; i++)
            {
                int saved = PlayerPrefs.GetInt($"StarCollected_{i}", 0);
                collectedStars[i] = saved == 1;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectStar(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= collectedStars.Length)
            return;

        collectedStars[chapterIndex] = true;
        PlayerPrefs.SetInt($"StarCollected_{chapterIndex}", 1);
        PlayerPrefs.Save();
        OnStarCollected?.Invoke(chapterIndex);

        int lastIndex = collectedStars.Length - 1;

        if (useGameFinishedUIForLastChapter && chapterIndex == lastIndex && GameFinishedUIManager.Instance != null)
        {
            GameFinishedUIManager.Instance.ShowGameFinished();
            return;
        }

        if (autoLoadNextSceneOnCollect && chapterIndex >= 0 && chapterIndex < nextSceneNames.Length)
        {
            string nextSceneName = nextSceneNames[chapterIndex];
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                StartCoroutine(LoadSceneAfterDelay(nextSceneName, loadNextSceneDelay));
            }
        }
    }

    public bool AllStarsCollected()
    {
        return collectedStars[0] && collectedStars[1] && collectedStars[2];
    }

    public int GetCollectedStarCount()
    {
        int count = 0;
        foreach (bool collected in collectedStars)
        {
            if (collected) count++;
        }
        return count;
    }

    private System.Collections.IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(sceneName);
    }
}
