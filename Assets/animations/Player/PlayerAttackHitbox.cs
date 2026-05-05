using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public int normalAttackDamage = 10;
    public int flamejetDamage = 15;

    [Header("Attack Type")]
    public AttackType attackType = AttackType.Normal;

    public enum AttackType
    {
        Normal,
        Flamejet
    }

    [Header("Hitbox Collider (assign di Inspector, biasanya collider di child 'HitBox')")]
    public Collider2D hitboxCollider;

    void Awake()
    {
        // Matikan collider hitbox saat start (jangan matikan collider body)
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    // Dipanggil dari Animation Event di animasi Attack / Flamejet
    public void EnableHitbox()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
        }
    }

    // Dipanggil dari Animation Event di animasi Attack / Flamejet
    public void DisableHitbox()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hitboxCollider != null && !hitboxCollider.enabled)
            return;

        int damage = GetDamage();

        if (other.CompareTag("Boss"))
        {
            BossHealth bossHealth = other.GetComponent<BossHealth>();
            if (bossHealth != null && !bossHealth.IsDead())
            {
                bossHealth.TakeDamage(damage);
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = other.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                return;
            }

            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth == null) enemyHealth = other.GetComponentInParent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }

    int GetDamage()
    {
        switch (attackType)
        {
            case AttackType.Normal:
                return normalAttackDamage;
            case AttackType.Flamejet:
                return flamejetDamage;
            default:
                return normalAttackDamage;
        }
    }
}


