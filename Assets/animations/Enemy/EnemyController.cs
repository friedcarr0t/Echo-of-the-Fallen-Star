using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Animator (Optional)")]
    public Animator animator;
    public string walkBoolParam = "isWalking";
    public string attackTriggerParam = "Attack";
    public bool driveAnimator = true;

    [Header("Facing")]
    [Tooltip("Kalau sprite default-nya menghadap kanan saat tidak di-flip, set true. Kalau default-nya menghadap kiri, set false.")]
    public bool spriteFacesRightWhenScalePositive = true;
    public SpriteRenderer spriteRenderer;
    [Tooltip("Kalau true, posisi AttackPoint akan dimirror kiri/kanan mengikuti facing.")]
    public bool mirrorAttackPoint = true;

    [Header("Target")]
    public Transform player;
    public float detectRange = 10f;
    public float attackRange = 1.8f;
    public float retreatRange = 1.2f; // Jarak terlalu dekat, stop dan attack (fix teleport bug)
    public float maxDetectVerticalDelta = 999f; // allow chase even if not perfectly same Y
    public float maxAttackVerticalDelta = 3f; // Diperbesar sedikit untuk fix bug attack hanya saat jump

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float retreatSpeed = 2.0f;
    public float retreatDuration = 0.6f;
    public float idleAfterAttack = 0.25f;

    [Header("Attack")]
    public Collider2D attackCollider; // biasanya collider trigger di child "AttackPoint"
    public float attackCooldown = 1.2f;
    public float attackActiveTime = 0.2f;
    [Tooltip("Kalau true, window hitbox serangan DIKONTROL oleh Animation Event (Enable/DisableAttackHitbox), bukan oleh coroutine AttackWindow.")]
    public bool useAnimationEventsForAttackWindow = false;

    [Header("Debug")]
    public bool enableDebugLogs = false; // Set true untuk enable debug logs (default: false)

    private Rigidbody2D rb;
    private Health health;
    private PlayerHealth playerHealth;
    private float lastAttackTime = -999f;
    private bool facingRight = true;
    private Coroutine attackRoutine;
    private bool hasWalkBoolParam = false;
    private bool hasAttackTriggerParam = false;
    private EnemyAttackHitbox attackHitbox;
    private Transform attackPointTransform;
    private Vector3 attackPointLocalPos;

    private enum EnemyState { Idle, Chase, Attack, Retreat }
    [SerializeField] private EnemyState state = EnemyState.Idle;
    private float stateTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Cache PlayerHealth supaya mudah cek apakah player sudah mati
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = player.GetComponentInParent<PlayerHealth>();
        }

        if (attackCollider == null)
        {
            Transform ap = transform.Find("AttackPoint");
            if (ap != null)
            {
                attackCollider = ap.GetComponent<Collider2D>();
            }
        }

        if (attackCollider != null)
        {
            // Hitbox script meng-handle collider enabled + window sendiri
            attackHitbox = attackCollider.GetComponent<EnemyAttackHitbox>();
            if (attackHitbox == null)
                attackHitbox = attackCollider.GetComponentInChildren<EnemyAttackHitbox>();

            attackPointTransform = attackCollider.transform;
            attackPointLocalPos = attackPointTransform.localPosition;
        }

        // Hindari negative scale di root karena bisa mirror collider dan bikin "teleport" saat collision resolution.
        Vector3 rootScale = transform.localScale;
        rootScale.x = Mathf.Abs(rootScale.x);
        transform.localScale = rootScale;

        // Inisialisasi facing berdasarkan SpriteRenderer.flipX (kalau ada), tanpa menyentuh collider.
        facingRight = GetFacingFromSprite();

        // Biar saat start langsung menghadap player
        if (player != null) FacePlayer();

        CacheAnimatorParams();
    }

    void Update()
    {
        if (player == null) return;

        // Jika player sudah mati, enemy berhenti bergerak & berhenti menyerang
        if (playerHealth != null && playerHealth.IsDead())
        {
            StopMoving();
            SetWalking(false);
            if (attackHitbox != null) attackHitbox.EndAttackWindow();
            return;
        }

        if (health != null && health.IsDead())
        {
            StopMoving();
            if (attackHitbox != null) attackHitbox.EndAttackWindow();
            SetWalking(false);
            return;
        }

        // Jarak horizontal dan total ke player
        float dx = player.position.x - transform.position.x;
        float horizontalDistance = Mathf.Abs(dx);
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Deteksi bisa pakai jarak total (biar enemy mulai ngejar saat cukup dekat),
        // tapi untuk memutuskan "bisa attack atau terlalu dekat", kita pakai
        // jarak horizontal saja supaya perbedaan tinggi / pivot tidak bikin bug
        // "enemy cuma nyerang kalau player lompat".
        bool detected = distanceToPlayer <= detectRange;
        bool inAttackRange = horizontalDistance <= attackRange;
        bool tooClose = horizontalDistance <= retreatRange;

        // Debug log disabled by default (set enableDebugLogs = true di Inspector jika perlu)

        if (stateTimer > 0f)
            stateTimer -= Time.deltaTime;

        // State machine (simple, "natural": chase -> attack -> retreat -> pause -> chase)
        switch (state)
        {
            case EnemyState.Idle:
                StopMoving();
                SetWalking(false);

                if (stateTimer <= 0f && detected)
                {
                    SetState(EnemyState.Chase, 0f);
                }
                break;

            case EnemyState.Chase:
                if (!detected)
                {
                    SetState(EnemyState.Idle, 0f);
                    break;
                }

                // Terlalu dekat - stop dan attack atau retreat (fix teleport bug)
                // JANGAN update facing saat terlalu dekat untuk mencegah flip-flop teleport
                if (tooClose)
                {
                    StopMoving();
                    SetWalking(false);

                    if (CanAttack())
                    {
                        DoAttack();
                        SetState(EnemyState.Attack, attackActiveTime);
                    }
                    else if (Random.value < 0.3f)
                    {
                        // Retreat jika tidak bisa attack (seperti boss)
                        SetState(EnemyState.Retreat, retreatDuration);
                    }
                    // Jika tidak bisa attack dan tidak retreat, tetap stop untuk hindari teleport
                    return; // Penting: return seperti boss untuk mencegah chase
                }

                // Selalu face player terlebih dahulu (seperti boss) untuk konsistensi
                // Tapi hanya jika tidak terlalu dekat (untuk mencegah flip-flop)
                FacePlayer();

                // Dalam attack range - stop dan attack atau retreat random
                if (inAttackRange)
                {
                    StopMoving();
                    SetWalking(false);
                    
                    if (CanAttack())
                    {
                        DoAttack();
                        SetState(EnemyState.Attack, attackActiveTime);
                    }
                    else
                    {
                        // Random retreat saat dalam attack range tapi tidak bisa attack
                        if (Random.value < 0.4f) // 40% chance retreat
                        {
                            SetState(EnemyState.Retreat, retreatDuration);
                        }
                    }
                    return; // Return untuk mencegah chase
                }

                // Di luar attack range - chase player
                // Gunakan facingRight untuk konsistensi (seperti boss)
                float dir = facingRight ? 1f : -1f;
                float chaseVel = dir * moveSpeed;
                if (rb != null)
                    rb.linearVelocity = new Vector2(chaseVel, rb.linearVelocity.y);
                SetWalking(true);
                break;

            case EnemyState.Attack:
                StopMoving();
                SetWalking(false);
                FacePlayer(); // Face player selama attack

                if (stateTimer <= 0f)
                    SetState(EnemyState.Retreat, retreatDuration);
                break;

            case EnemyState.Retreat:
                // move away from player for a short time
                // Gunakan facingRight untuk konsistensi (seperti boss)
                float retreatDir = facingRight ? -1f : 1f; // Mundur = kebalikan facing
                if (rb != null)
                    rb.linearVelocity = new Vector2(retreatDir * retreatSpeed, rb.linearVelocity.y);
                FacePlayer(); // Tetap face player saat retreat
                SetWalking(true);

                if (stateTimer <= 0f)
                {
                    SetState(EnemyState.Idle, idleAfterAttack);
                }
                break;
        }
    }

    void SetState(EnemyState newState, float timer)
    {
        state = newState;
        stateTimer = timer;

        if (newState != EnemyState.Attack && attackHitbox != null)
            attackHitbox.EndAttackWindow();
    }

    // ChasePlayer tidak lagi digunakan - logic dipindah langsung ke state Chase untuk konsistensi dengan boss

    void StopMoving()
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    void DoAttack()
    {
        lastAttackTime = Time.time;

        TriggerAttack();

        // Kalau tidak pakai Animation Event, pakai fallback coroutine window bawaan
        if (!useAnimationEventsForAttackWindow)
        {
            if (attackRoutine != null)
                StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(AttackWindow());
        }
    }

    IEnumerator AttackWindow()
    {
        if (attackHitbox != null)
            attackHitbox.BeginAttackWindow();

        yield return new WaitForSeconds(attackActiveTime);

        if (attackHitbox != null)
            attackHitbox.EndAttackWindow();

        attackRoutine = null;
    }

    void FacePlayer()
    {
        if (player == null) return;
        
        float dir = player.position.x - transform.position.x;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Lock facing saat dalam range yang lebih besar untuk mencegah flip-flop teleport
        // Gunakan 2x attackRange sebagai zona "lock facing" untuk mencegah flip-flop saat mendekati
        float lockFacingRange = attackRange * 2f; // ~3.6 untuk attackRange 1.8
        
        if (distanceToPlayer <= lockFacingRange)
        {
            return;
        }
        
        // Threshold lebih besar untuk mencegah flip-flop terlalu cepat (seperti boss)
        // Gunakan threshold yang lebih besar saat mendekati lock range
        float threshold = distanceToPlayer < lockFacingRange * 1.2f ? 0.5f : 0.1f;
        if (Mathf.Abs(dir) < threshold)
        {
            return;
        }
        FaceDirection(Mathf.Sign(dir));
    }

    void FaceDirection(float dir)
    {
        bool shouldFaceRight = dir > 0f;
        if (shouldFaceRight == facingRight) return;

        SetFacing(shouldFaceRight);
    }

    bool GetFacingFromSprite()
    {
        if (spriteRenderer == null) return true;

        // Mapping flipX -> facingRight
        // - Jika default sprite menghadap kanan: flipX=false => right, flipX=true => left
        // - Jika default sprite menghadap kiri : flipX=false => left , flipX=true => right
        return spriteFacesRightWhenScalePositive ? !spriteRenderer.flipX : spriteRenderer.flipX;
    }

    void SetFacing(bool faceRight)
    {
        facingRight = faceRight;

        // Flip visual via SpriteRenderer (tidak mempengaruhi collider/physics)
        if (spriteRenderer != null)
        {
            // If default faces right: facingRight -> flipX false
            // If default faces left : facingRight -> flipX true
            spriteRenderer.flipX = spriteFacesRightWhenScalePositive ? !faceRight : faceRight;
        }

        // Mirror AttackPoint position so hitbox stays on correct side
        if (mirrorAttackPoint && attackPointTransform != null)
        {
            Vector3 p = attackPointLocalPos;
            p.x = Mathf.Abs(attackPointLocalPos.x) * (faceRight ? 1f : -1f);
            attackPointTransform.localPosition = p;
        }
    }

    void CacheAnimatorParams()
    {
        if (!driveAnimator || animator == null) return;

        hasWalkBoolParam = HasParam(animator, walkBoolParam, AnimatorControllerParameterType.Bool);
        hasAttackTriggerParam = HasParam(animator, attackTriggerParam, AnimatorControllerParameterType.Trigger);
    }

    static bool HasParam(Animator anim, string name, AnimatorControllerParameterType type)
    {
        if (anim == null || string.IsNullOrEmpty(name)) return false;
        foreach (var p in anim.parameters)
        {
            if (p.type == type && p.name == name) return true;
        }
        return false;
    }

    void SetWalking(bool walking)
    {
        if (!driveAnimator || animator == null || !hasWalkBoolParam) return;
        animator.SetBool(walkBoolParam, walking);
    }

    void TriggerAttack()
    {
        if (!driveAnimator || animator == null || !hasAttackTriggerParam) return;
        animator.ResetTrigger(attackTriggerParam);
        animator.SetTrigger(attackTriggerParam);
    }
}


