using UnityEngine;
using UnityEngine.UI;

public class StarCounterUI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] starImages; // Array untuk 3 star images (Chapter 1, 2, 3)
    public Text starCountText; // Optional: untuk menampilkan "X/3 Stars"

    [Header("Star Settings")]
    public Sprite starCollectedSprite; // Sprite untuk star yang sudah dikoleksi
    public Sprite starEmptySprite; // Sprite untuk star yang belum dikoleksi
    public Color collectedColor = Color.yellow;
    public Color emptyColor = Color.gray;

    private StarManager starManager;

    void Start()
    {
        // Cari StarManager
        starManager = StarManager.Instance;

        // Setup star images
        if (starImages != null && starImages.Length > 0)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].color = emptyColor;
                    if (starEmptySprite != null)
                    {
                        starImages[i].sprite = starEmptySprite;
                    }
                }
            }
        }

        UpdateStarDisplay();
    }

    void OnEnable()
    {
        // Subscribe ke event StarManager
        StarManager.OnStarCollected += OnStarCollected;
    }

    void OnDisable()
    {
        // Unsubscribe dari event
        StarManager.OnStarCollected -= OnStarCollected;
    }

    void Update()
    {
        // Pastikan reference ke StarManager selalu terisi (misalnya saat pindah scene)
        if (starManager == null && StarManager.Instance != null)
        {
            starManager = StarManager.Instance;
        }

        // Update setiap frame untuk memastikan UI selalu sync
        if (starManager != null)
            UpdateStarDisplay();
    }

    void OnStarCollected(int chapterIndex)
    {
        // Update UI ketika star dikoleksi
        UpdateStarDisplay();
    }

    void UpdateStarDisplay()
    {
        if (starManager == null) return;

        int collectedCount = 0;

        // Update setiap star image
        for (int i = 0; i < starImages.Length && i < starManager.collectedStars.Length; i++)
        {
            if (starImages[i] != null)
            {
                bool isCollected = starManager.collectedStars[i];

                if (isCollected)
                {
                    collectedCount++;
                    starImages[i].color = collectedColor;
                    if (starCollectedSprite != null)
                    {
                        starImages[i].sprite = starCollectedSprite;
                    }
                }
                else
                {
                    starImages[i].color = emptyColor;
                    if (starEmptySprite != null)
                    {
                        starImages[i].sprite = starEmptySprite;
                    }
                }
            }
        }

        // Update text (jika ada)
        if (starCountText != null)
        {
            starCountText.text = $"Stars: {collectedCount} / {starManager.collectedStars.Length}";
        }
    }
}

