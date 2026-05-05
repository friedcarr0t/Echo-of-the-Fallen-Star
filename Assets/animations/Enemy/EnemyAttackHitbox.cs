using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;

    [Header("Hit Window")]
    public bool canDamage = false; // diaktifkan oleh EnemyController saat attack window

    private bool didDamageThisWindow = false;
    private Collider2D hitboxCollider;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();

        // Supaya event trigger stabil, collider trigger ini lebih aman selalu enabled,
        // damage-nya yang kita gate via canDamage.
        if (hitboxCollider != null)
            hitboxCollider.enabled = true;
    }

    public void BeginAttackWindow()
    {
        canDamage = true;
        didDamageThisWindow = false;
    }

    public void EndAttackWindow()
    {
        canDamage = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Ini yang bikin hit tetap jalan walau collider sudah overlap sebelum attack window dibuka.
        TryDamage(other);
    }

    void TryDamage(Collider2D other)
    {
        if (!canDamage) return;
        if (didDamageThisWindow) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null) return;
        if (playerHealth.IsDead()) return;

        playerHealth.TakeDamage(damage);
        didDamageThisWindow = true;
    }
}


