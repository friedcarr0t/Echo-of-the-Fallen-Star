using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public Image fillImage;
    public Text healthText; // Optional: untuk menampilkan angka HP
    public GameObject healthBarPanel; // Panel yang berisi health bar (untuk show/hide)
    public CanvasGroup canvasGroup;   // Dipakai untuk show/hide tanpa mematikan GameObject (aman untuk script)

    [Header("Color Settings")]
    public Color fullHealthColor = Color.red;
    public Color lowHealthColor = Color.yellow;
    public float lowHealthThreshold = 0.3f; // 30% HP

    [Header("Boss Settings")]
    public string bossTag = "Boss"; // Tag untuk mencari boss

    [Header("Debug")]
    public bool debugLogs = false;

    private BossHealth bossHealth;
    private bool bossFound = false;

    void Awake()
    {
        if (debugLogs) Debug.Log("[BossHealthBarUI] Awake called!");
    }

    void Start()
    {
        if (debugLogs) Debug.Log("[BossHealthBarUI] Start called!");

        // Pastikan kita tidak mematikan GameObject yang menampung script ini.
        // Banyak scene/prefab meng-assign healthBarPanel = gameObject (self), jadi SetActive(false)
        // akan mematikan Update() dan bar tidak akan pernah muncul lagi.
        EnsureCanvasGroup();
        SetPanelVisible(false);

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 200;
        }
    }

    void Update()
    {
        // Cari boss jika belum ketemu
        if (!bossFound || bossHealth == null)
        {
            GameObject boss = GameObject.FindGameObjectWithTag(bossTag);
            if (boss != null)
            {
                bossHealth = boss.GetComponent<BossHealth>();
                if (bossHealth != null)
                {
                    bossFound = true;
                    if (debugLogs) Debug.Log($"[BossHealthBarUI] Boss found: {boss.name}");
                }
            }
        }

        // Jika boss belum ketemu, sembunyikan
        if (!bossFound || bossHealth == null)
        {
            SetPanelVisible(false);
            return;
        }

        // Cek jika boss mati
        if (bossHealth.IsDead())
        {
            SetPanelVisible(false);
            return;
        }

        // Cek apakah boss visible di camera
        bool bossVisible = IsBossVisible();
        if (debugLogs) Debug.Log($"[BossHealthBarUI] Boss visible: {bossVisible}, HP: {bossHealth.GetCurrentHealth()}/{bossHealth.GetMaxHealth()}");

        SetPanelVisible(bossVisible);

        if (bossVisible)
        {
            UpdateHealthBar();
        }
    }

    bool IsBossVisible()
    {
        if (bossHealth == null) return false;

        Camera cam = Camera.main;
        if (cam == null) return false;

        // Lebih akurat dari sekadar pivot: pakai bounds renderer (kalau ada).
        // Ini bikin bar muncul saat sprite boss mulai terlihat (walau pivot/transform masih di luar viewport).
        Renderer r = bossHealth.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            return GeometryUtility.TestPlanesAABB(planes, r.bounds);
        }

        Vector3 viewPos = cam.WorldToViewportPoint(bossHealth.transform.position);
        return viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0;
    }

    void UpdateHealthBar()
    {
        if (bossHealth == null) return;

        int currentHealth = bossHealth.GetCurrentHealth();
        int maxHealth = bossHealth.GetMaxHealth();

        // Update slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Update text (jika ada)
        if (healthText != null)
        {
            healthText.text = $"Boss: {currentHealth} / {maxHealth}";
        }

        // Update color berdasarkan HP
        if (fillImage != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            
            if (healthPercent <= lowHealthThreshold)
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent / lowHealthThreshold);
            }
            else
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, (healthPercent - lowHealthThreshold) / (1f - lowHealthThreshold));
            }
        }
    }

    void EnsureCanvasGroup()
    {
        GameObject target = healthBarPanel != null ? healthBarPanel : gameObject;
        if (canvasGroup == null && target != null)
            canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null && target != null)
            canvasGroup = target.AddComponent<CanvasGroup>();
    }

    void SetPanelVisible(bool visible)
    {
        EnsureCanvasGroup();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            return;
        }

        // Fallback (kalau CanvasGroup gagal): hanya aktifkan objek panel jika itu bukan object yang sama dengan script.
        if (healthBarPanel != null && healthBarPanel != gameObject)
        {
            healthBarPanel.SetActive(visible);
        }
        else
        {
            // Jangan pernah mematikan gameObject sendiri, karena itu akan mematikan Update().
            if (debugLogs && !visible)
                Debug.LogWarning("[BossHealthBarUI] healthBarPanel menunjuk ke object yang sama dengan script; memakai fallback tanpa SetActive(false).");
        }
    }
}


