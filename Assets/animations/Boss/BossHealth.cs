using UnityEngine;

public class BossHealth : MonoBehaviour
{
    private Health health;
    private BossController bossController;
    private Animator anim;

    [Header("Drop Settings")]
    public GameObject starPrefab;
    public int starDropCount = 3;
    public float dropSpread = 1f;
    public float deathDelay = 1.5f; // Waktu tunggu sebelum drop dan destroy

    private bool hasDied = false;

    void Start()
    {
        health = GetComponent<Health>();
        bossController = GetComponent<BossController>();
        anim = GetComponent<Animator>();
        
        if (health == null)
        {
            health = gameObject.AddComponent<Health>();
            health.maxHealth = 200;
            health.hurtTrigger = "hit";
            health.deadTrigger = "death";
        }
    }

    public void TakeDamage(int damage)
    {
        if (health == null || hasDied) return;
        if (bossController != null && bossController.currentState == BossController.BossState.Dead) return;

        health.TakeDamage(damage);

        if (health.IsDead())
        {
            Die();
        }
        else
        {
            // Boss kena hit - trigger hurt animation langsung
            if (anim != null)
            {
                anim.ResetTrigger("hit");
                anim.SetTrigger("hit");
            }
            if (bossController != null)
            {
                bossController.OnHurt();
            }
        }
    }

    void Die()
    {
        if (hasDied) return;
        hasDied = true;

        // Trigger death animation
        if (anim != null)
        {
            anim.ResetTrigger("death");
            anim.SetTrigger("death");
        }

        // Set boss state
        if (bossController != null)
        {
            bossController.currentState = BossController.BossState.Dead;
            bossController.enabled = false;
        }

        // Drop star setelah delay (animasi death selesai)
        Invoke(nameof(DropStarsAndDestroy), deathDelay);
    }

    void DropStarsAndDestroy()
    {
        // Drop stars
        if (starPrefab != null)
        {
            for (int i = 0; i < starDropCount; i++)
            {
                Vector3 dropPos = transform.position + new Vector3(
                    Random.Range(-dropSpread, dropSpread),
                    Random.Range(0.5f, 1.5f),
                    0
                );
                GameObject star = Instantiate(starPrefab, dropPos, Quaternion.identity);
                
                // Mark as dropped item
                StarFragment fragment = star.GetComponent<StarFragment>();
                if (fragment != null)
                {
                    fragment.isDroppedItem = true;
                }
                
                // Beri sedikit velocity ke atas
                Rigidbody2D starRb = star.GetComponent<Rigidbody2D>();
                if (starRb != null)
                {
                    starRb.linearVelocity = new Vector2(Random.Range(-2f, 2f), Random.Range(3f, 5f));
                }
            }
        }

        // Destroy boss
        Destroy(gameObject);
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
        return health != null ? health.maxHealth : 200;
    }
}

