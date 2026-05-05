using UnityEngine;

/// <summary>
/// Script sederhana untuk dipakai di Scene 2:
/// - Pasang script ini di GameObject enemy yang punya Animator.
/// - Assign field "Attack Hitbox" ke child yang punya EnemyAttackHitbox
///   (biasanya GameObject "AttackPoint").
/// - Di animasi serangan, tambahkan Animation Event:
///     - Di frame serangan mulai  -> panggil EnableAttackHitbox()
///     - Di frame serangan selesai -> panggil DisableAttackHitbox()
/// </summary>
public class EnemyAnimationEvents : MonoBehaviour
{
    [Header("Reference ke Script Hitbox")]
    public EnemyAttackHitbox attackHitbox;

    void Awake()
    {
        // Kalau belum di-assign di Inspector, coba cari otomatis di child
        if (attackHitbox == null)
        {
            attackHitbox = GetComponentInChildren<EnemyAttackHitbox>();
            if (attackHitbox == null)
            {
                Debug.LogWarning("[EnemyAnimationEvents] EnemyAttackHitbox TIDAK ditemukan di children! Assign manual di Inspector.");
            }
        }
    }

    // ======= Dipanggil dari Animation Event =======

    // Panggil di frame saat serangan mulai "kena"
    public void EnableAttackHitbox()
    {
        if (attackHitbox == null)
        {
            Debug.LogWarning("[EnemyAnimationEvents] EnableAttackHitbox dipanggil tapi attackHitbox = null");
            return;
        }

        attackHitbox.BeginAttackWindow();
    }

    // Panggil di frame saat serangan sudah selesai
    public void DisableAttackHitbox()
    {
        if (attackHitbox == null)
        {
            Debug.LogWarning("[EnemyAnimationEvents] DisableAttackHitbox dipanggil tapi attackHitbox = null");
            return;
        }

        attackHitbox.EndAttackWindow();
    }
}


