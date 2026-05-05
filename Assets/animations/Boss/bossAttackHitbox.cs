using UnityEngine;

public class BossAttackHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 20;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Hanya proses jika tag Player
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null) return;

        if (!playerHealth.IsDead())
        {
            Debug.Log($"Boss menyerang player dengan damage: {damage}");
            playerHealth.TakeDamage(damage);
        }
    }
}

