using UnityEngine;

/// <summary>
/// Script helper untuk Animation Event di animasi Player:
/// - Dipasang di GameObject yang punya Animator (biasanya root "Player").
/// - Di Inspector, assign reference ke PlayerAttackHitbox (child yang punya script itu).
/// - Di clip animasi serangan (attack / flameJet / dsb), panggil:
///     - EnableHitbox()  di frame serangan mulai kena
///     - DisableHitbox() di frame serangan selesai
/// </summary>
public class PlayerAnimationEvents : MonoBehaviour
{
    [Header("Reference ke Script Hitbox Player")]
    public PlayerAttackHitbox attackHitbox;

    void Awake()
    {
        // Kalau belum di-assign, coba cari otomatis di children
        if (attackHitbox == null)
        {
            attackHitbox = GetComponentInChildren<PlayerAttackHitbox>();
            if (attackHitbox == null)
            {
                Debug.LogWarning("[PlayerAnimationEvents] PlayerAttackHitbox tidak ditemukan di children! Assign manual di Inspector.");
            }
        }
    }

    // ===== DIPANGGIL DARI ANIMATION EVENT =====

    // Panggil di awal window serangan
    public void EnableHitbox()
    {
        if (attackHitbox == null)
        {
            Debug.LogWarning("[PlayerAnimationEvents] EnableHitbox dipanggil tapi attackHitbox = null");
            return;
        }

        attackHitbox.EnableHitbox();
    }

    // Panggil di akhir window serangan
    public void DisableHitbox()
    {
        if (attackHitbox == null)
        {
            Debug.LogWarning("[PlayerAnimationEvents] DisableHitbox dipanggil tapi attackHitbox = null");
            return;
        }

        attackHitbox.DisableHitbox();
    }
}


