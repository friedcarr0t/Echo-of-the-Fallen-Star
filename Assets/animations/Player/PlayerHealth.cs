using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private Health health;

    void Awake()
    {
        health = GetComponent<Health>();
        
        if (health == null)
        {
            health = gameObject.AddComponent<Health>();
        }
        
        health.maxHealth = 100;
        health.currentHealth = 100;
        
        // Set trigger name yang benar untuk player animator
        // Player animator menggunakan "Trigger Dead" bukan "Dead"
        health.hurtTrigger = "Trigger Hurt";
        health.deadTrigger = "Trigger Dead"; // Player animator menggunakan "Trigger Dead"
    }

    public void TakeDamage(int damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }

    public bool IsDead()
    {
        return health != null && health.IsDead();
    }

    public int GetCurrentHealth()
    {
        return health != null ? health.currentHealth : 0;
    }

    public int GetMaxHealth()
    {
        return health != null ? health.maxHealth : 100;
    }
}


