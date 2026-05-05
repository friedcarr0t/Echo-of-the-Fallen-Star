using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthSlider;
    public Image fillImage;
    public Text healthText;

    [Header("Color Settings")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public float lowHealthThreshold = 0.3f;

    private PlayerHealth playerHealth;

    void Start()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 100;
        }
        
        // Cari player di Start untuk memastikan ditemukan
        FindPlayer();
    }

    void Update()
    {
        if (playerHealth == null)
        {
            FindPlayer();
        }

        if (playerHealth != null)
        {
            UpdateHealthBar();
        }
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Cari PlayerHealth di parent dulu
            playerHealth = player.GetComponent<PlayerHealth>();
            
            // Jika tidak ada, cari di children
            if (playerHealth == null)
            {
                playerHealth = player.GetComponentInChildren<PlayerHealth>();
            }
            
        }
    }

    void UpdateHealthBar()
    {
        int currentHealth = playerHealth.GetCurrentHealth();
        int maxHealth = playerHealth.GetMaxHealth();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }

        if (fillImage != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            
            if (healthPercent <= lowHealthThreshold)
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent / lowHealthThreshold);
            }
            else
            {
                fillImage.color = fullHealthColor;
            }
        }
    }
}
