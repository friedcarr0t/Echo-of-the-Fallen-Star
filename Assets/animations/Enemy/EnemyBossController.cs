using UnityEngine;

// Boss-like AI for normal enemies (no boss UI). Designed to feel similar to BossController:
// - Idle -> Chase -> Attack -> Retreat -> Chase
// - Attack window controlled by animation events EnableAttack/DisableAttack (fallback supported)
public class EnemyBossController : MonoBehaviour
{
    public enum EnemyState { Idle, Chase, Retreat, Attack, Hurt, Dead }
    public EnemyState currentState = EnemyState.Idle;

    [Header("Target")]
    public Transform player;
    public float detectRange = 10f;
    public float attackRange = 1.8f;
    public float retreatRange = 1.2f;
    public float maxDetectVerticalDelta = 999f;
    public float maxAttackVerticalDelta = 2.5f;

    [Header("Movement")]
    public float chaseSpeed = 2.5f;
    public float retreatSpeed = 2.0f;
    public float retreatDuration = 0.6f;
    public float idleAfterAttack = 0.25f;

    [Header("Combat Timing")]
    public float attackCooldown = 1.2f;

    [Header("Animator Params")]
    public string runningBool = "isRunning";
    public string attackTrigger = "Attack";
    public string hurtTrigger = "GetHit";
    public string deathStateName = "Death"; // for anim.Play

    [Header("Facing (visual only)")]
    public SpriteRenderer spriteRenderer;
    public bool defaultFacesRight = false; // many pixel enemies face left by default

    [Header("Attack Point")]
    public EnemyAttackHitbox attackHitbox; // should live on AttackPoint
    public Transform attackPoint;
    public bool mirrorAttackPoint = true;

    [Header("Fallback attack window (if no anim events)")]
    public float fallbackAttackActiveTime = 0.2f;

    [Header("Debug")]
    public bool enableDebugLogs = false; // Set true untuk enable debug logs (default: false)

    private Rigidbody2D rb;
    private Animator anim;
    private Health health;
    private float lastAttackTime = -999f;
    private float stateTimer = 0f;
    private bool facingRight = false;
    private Vector3 attackPointLocalPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (attackHitbox == null)
        {
            // Try find AttackPoint child
            Transform ap = transform.Find("AttackPoint");
            if (ap != null)
            {
                attackPoint = ap;
                attackHitbox = ap.GetComponent<EnemyAttackHitbox>();
                if (attackHitbox == null) attackHitbox = ap.GetComponentInChildren<EnemyAttackHitbox>();
            }
        }
        if (attackPoint == null && attackHitbox != null)
            attackPoint = attackHitbox.transform;

        if (attackPoint != null)
            attackPointLocalPos = attackPoint.localPosition;

        // Ensure root scale stays positive to avoid mirrored physics shapes
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;

        // Init facing toward player if possible
        if (player != null)
            FacePlayer();
    }

    void Update()
    {
        if (player == null) return;

        if (health != null && health.IsDead())
        {
            Die();
            return;
        }

        // Gunakan Vector2.Distance seperti boss untuk akurasi lebih baik (fix bug teleport & attack)
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float dx = player.position.x - transform.position.x;
        float ady = Mathf.Abs(player.position.y - transform.position.y);

        // Deteksi menggunakan jarak total (seperti boss)
        bool detected = distanceToPlayer <= detectRange && ady <= maxDetectVerticalDelta;
        bool inAttack = distanceToPlayer <= attackRange && ady <= maxAttackVerticalDelta;
        bool tooClose = distanceToPlayer <= retreatRange && ady <= maxAttackVerticalDelta;

        // Debug log disabled by default (set enableDebugLogs = true di Inspector jika perlu)

        if (stateTimer > 0f) stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.Idle:
                StopX();
                SetRunning(false);
                if (detected)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                if (!detected)
                {
                    currentState = EnemyState.Idle;
                    break;
                }

                // Terlalu dekat - stop dan attack atau retreat (fix teleport bug)
                // JANGAN update facing saat terlalu dekat untuk mencegah flip-flop teleport
                if (tooClose)
                {
                    StopX();
                    SetRunning(false);

                    if (CanAttack())
                    {
                        StartAttack();
                    }
                    else if (Random.value < 0.3f)
                    {
                        // Retreat jika tidak bisa attack (seperti boss)
                        currentState = EnemyState.Retreat;
                        stateTimer = retreatDuration;
                    }
                    // Jika tidak bisa attack dan tidak retreat, tetap stop untuk hindari teleport
                    return; // Penting: return untuk mencegah chase
                }

                // Selalu face player terlebih dahulu (seperti boss) untuk konsistensi
                // Tapi hanya jika tidak terlalu dekat (untuk mencegah flip-flop)
                FacePlayer();

                // Dalam attack range - stop dan attack atau retreat random
                if (inAttack)
                {
                    StopX();
                    SetRunning(false);
                    
                    if (CanAttack())
                    {
                        StartAttack();
                    }
                    else
                    {
                        // Random retreat saat dalam attack range tapi tidak bisa attack
                        if (Random.value < 0.4f) // 40% chance retreat
                        {
                            currentState = EnemyState.Retreat;
                            stateTimer = retreatDuration;
                        }
                    }
                    return; // Return untuk mencegah chase
                }

                // Di luar attack range - chase player
                // Gunakan facingRight untuk konsistensi (seperti boss)
                float chaseDir = facingRight ? 1f : -1f;
                float chaseVel = chaseDir * chaseSpeed;
                MoveX(chaseVel);
                SetRunning(true);
                break;

            case EnemyState.Attack:
                StopX();
                SetRunning(false);
                FacePlayer(); // Face player selama attack (konsisten dengan boss)

                // if animator event is missing, fallback timer will move us forward
                if (stateTimer <= 0f)
                {
                    currentState = EnemyState.Retreat;
                    stateTimer = retreatDuration;
                }
                break;

            case EnemyState.Retreat:
                // move away from player for a moment
                float retreatDir = facingRight ? -1f : 1f;
                MoveX(retreatDir * retreatSpeed);
                SetRunning(true);
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    currentState = EnemyState.Idle;
                    stateTimer = idleAfterAttack;
                    StopX();
                    SetRunning(false);
                }
                break;

            case EnemyState.Hurt:
                StopX();
                SetRunning(false);
                // optional: after short delay go chase
                if (stateTimer <= 0f)
                    currentState = EnemyState.Chase;
                break;
        }
    }

    bool CanAttack() => Time.time >= lastAttackTime + attackCooldown;

    void StartAttack()
    {
        lastAttackTime = Time.time;
        currentState = EnemyState.Attack;
        stateTimer = fallbackAttackActiveTime; // used if no anim events

        if (anim != null && !string.IsNullOrEmpty(attackTrigger))
        {
            anim.ResetTrigger(attackTrigger);
            anim.SetTrigger(attackTrigger);
        }

        // Fallback window (in case you didn't set animation events yet)
        if (attackHitbox != null)
        {
            attackHitbox.BeginAttackWindow();
            Invoke(nameof(DisableAttack), fallbackAttackActiveTime);
        }
    }

    void MoveX(float x)
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);
    }

    void StopX()
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void SetRunning(bool running)
    {
        if (anim == null || string.IsNullOrEmpty(runningBool)) return;
        anim.SetBool(runningBool, running);
    }

    void FacePlayer()
    {
        if (player == null) return;
        
        float dx = player.position.x - transform.position.x;
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
        if (Mathf.Abs(dx) < threshold)
        {
            return;
        }
        FaceDirection(dx);
    }

    void FaceDirection(float dx)
    {
        if (Mathf.Abs(dx) < 0.1f) return; // Threshold lebih besar
        bool newFacingRight = dx > 0f;
        if (newFacingRight != facingRight)
        {
            if (enableDebugLogs) Debug.Log($"[ENEMY DEBUG] {gameObject.name} | Facing Change | DX: {dx:F2} | " +
                                          $"Old: {(facingRight ? "Right" : "Left")} -> New: {(newFacingRight ? "Right" : "Left")}");
        }
        SetFacing(newFacingRight);
    }

    void SetFacing(bool faceRight)
    {
        facingRight = faceRight;

        if (spriteRenderer != null)
        {
            // If default faces right: facingRight => flipX false
            // If default faces left : facingRight => flipX true
            spriteRenderer.flipX = defaultFacesRight ? !facingRight : facingRight;
        }

        if (mirrorAttackPoint && attackPoint != null)
        {
            Vector3 p = attackPointLocalPos;
            p.x = Mathf.Abs(attackPointLocalPos.x) * (facingRight ? 1f : -1f);
            attackPoint.localPosition = p;
        }
    }

    // Called by animation events (recommended)
    public void EnableAttack()
    {
        if (attackHitbox != null) attackHitbox.BeginAttackWindow();
    }

    public void DisableAttack()
    {
        if (attackHitbox != null) attackHitbox.EndAttackWindow();
    }

    public void OnAttackFinished()
    {
        if (currentState != EnemyState.Attack) return;
        DisableAttack();
        currentState = EnemyState.Retreat;
        stateTimer = retreatDuration;
    }

    // Called by EnemyHealth (boss-like)
    public void OnHurt(float hurtDuration = 0.25f)
    {
        if (currentState == EnemyState.Dead) return;
        currentState = EnemyState.Hurt;
        stateTimer = hurtDuration;
        if (anim != null && !string.IsNullOrEmpty(hurtTrigger))
        {
            anim.ResetTrigger(hurtTrigger);
            anim.SetTrigger(hurtTrigger);
        }
    }

    void Die()
    {
        if (currentState == EnemyState.Dead) return;
        currentState = EnemyState.Dead;
        StopX();
        SetRunning(false);
        DisableAttack();

        if (anim != null && !string.IsNullOrEmpty(deathStateName))
            anim.Play(deathStateName, 0, 0f);
    }
}


