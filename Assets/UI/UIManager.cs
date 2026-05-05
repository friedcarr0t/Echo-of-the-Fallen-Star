using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Components")]
    public HealthBarUI healthBarUI;
    public StarCounterUI starCounterUI;

    public static UIManager Instance;

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
        // Validasi components
        if (healthBarUI == null)
        {
            Debug.LogWarning("HealthBarUI belum di-assign di UIManager!");
        }

        if (starCounterUI == null)
        {
            Debug.LogWarning("StarCounterUI belum di-assign di UIManager!");
        }
    }
}


