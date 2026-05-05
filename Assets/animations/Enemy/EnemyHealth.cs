using UnityEngine;

// Boss-like health wrapper for normal enemies:
// - Ensures Health exists, sets max/current HP (default 100)
// - On damage, notifies EnemyBossController for Hurt state
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public string hurtTrigger = "GetHit";
    public string deathStateName = "Death";

    [Header("Hurt Settings")]
    public float hurtDuration = 0.25f;

    private Health health;
    private EnemyBossController controller;
    private bool initialized = false;

    void Awake()
    {
        health = GetComponent<Health>();
        if (health == null)
            health = gameObject.AddComponent<Health>();

        controller = GetComponent<EnemyBossController>();

        ApplyDefaults();
    }

    void ApplyDefaults()
    {
        if (health == null) return;

        health.maxHealth = maxHealth;
        if (health.currentHealth < 0 || health.currentHealth > maxHealth)
            health.currentHealth = maxHealth;

        health.hurtTrigger = hurtTrigger;
        health.deadTrigger = deathStateName;

        initialized = true;
    }

    public void TakeDamage(int damage)
    {
        if (!initialized) ApplyDefaults();
        if (health == null || health.IsDead())
            return;

        health.TakeDamage(damage);

        if (!health.IsDead() && controller != null)
        {
            controller.OnHurt(hurtDuration);
        }
    }

    public bool IsDead() => health != null && health.IsDead();
    public int GetCurrentHealth() => health != null ? health.currentHealth : 0;
    public int GetMaxHealth() => health != null ? health.maxHealth : maxHealth;
}


