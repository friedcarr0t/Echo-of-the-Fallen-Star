using UnityEngine;

public class FireballDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 20;

    [Header("Charge Settings")]
    public float chargeDuration = 1.02f; // Durasi animasi charge fireball

    private Collider2D fireballCollider;
    private bool isCharging = true;

    void Start()
    {
        // Ambil collider
        fireballCollider = GetComponent<Collider2D>();
        
        // Nonaktifkan collider selama charge
        if (fireballCollider != null)
        {
            fireballCollider.enabled = false;
        }
        
        // Aktifkan collider setelah charge selesai
        Invoke("EnableCollider", chargeDuration);
    }

    void EnableCollider()
    {
        isCharging = false;
        if (fireballCollider != null)
        {
            fireballCollider.enabled = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Jangan proses damage jika masih charging
        if (isCharging) return;
        
        // Jangan damage player yang melempar
        if (other.CompareTag("Player")) return;

        bool hitTarget = false;

        // Damage Boss
        if (other.CompareTag("Boss"))
        {
            BossHealth bossHealth = other.GetComponent<BossHealth>();
            if (bossHealth != null && !bossHealth.IsDead())
            {
                bossHealth.TakeDamage(damage);
                hitTarget = true;
            }
        }

        // Damage Enemy/Monster
        if (other.CompareTag("Enemy"))
        {
            // Prefer EnemyHealth (boss-like wrapper) if present
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = other.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitTarget = true;
            }
            else
            {
                Health enemyHealth = other.GetComponent<Health>();
                if (enemyHealth == null) enemyHealth = other.GetComponentInParent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    hitTarget = true;
                }
            }
        }

        // Hancurkan fireball setelah hit target atau jika hit ground/wall
        if (hitTarget)
        {
            DestroyFireball();
        }
        else
        {
            // Hancurkan fireball jika hit sesuatu yang bukan karakter (ground/wall)
            // Cek jika tidak ada health component berarti itu ground/wall/obstacle
            if (other.GetComponent<BossHealth>() == null && 
                other.GetComponent<Health>() == null &&
                !other.CompareTag("Player"))
            {
                DestroyFireball();
            }
        }
    }

    void DestroyFireball()
    {
        // Optional: tambahkan efek visual sebelum destroy
        Destroy(gameObject);
    }
}


