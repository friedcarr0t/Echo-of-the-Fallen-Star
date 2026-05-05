using UnityEngine;

public class BossController : MonoBehaviour
{
    public enum BossState { Idle, Chase, Retreat, Attack1, Attack2, Jump, Hurt, Dead }
    public BossState currentState;

    [Header("Target")]
    public Transform player;
    public float detectRange = 10f;      // Jarak deteksi
    public float attackRange = 5f;       // Jarak untuk attack
    public float retreatRange = 1.5f;    // Jarak terlalu dekat, mundur

    [Header("Movement")]
    public float chaseSpeed = 3f;
    public float retreatSpeed = 2f;
    public float jumpForce = 8f;

    [Header("Combat Timing")]
    public float attackCooldown = 1.5f;  // Lebih cepat attack
    public float idleAfterAttack = 1f;   // Jeda setelah attack sebelum chase lagi
    public float retreatDuration = 0.8f;
    public float hurtDuration = 0.5f;

    [Header("AI Behavior")]
    [Range(0f, 1f)] public float attackChance = 0.8f;    // Lebih sering attack
    [Range(0f, 1f)] public float jumpChance = 0.1f;      // Kurangi jump

    [Header("Attack Point")]
    public GameObject attackPointObject;

    // Private
    private Rigidbody2D rb;
    private Animator anim;
    private BossHealth bossHealth;
    private bool facingRight = false;
    private float lastAttackTime;
    private float stateTimer;
    private bool isGrounded;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
        currentState = BossState.Idle;

        // Auto-find player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Auto-find AttackPoint
        if (attackPointObject == null)
        {
            Transform ap = transform.Find("AttackPoint");
            if (ap != null)
                attackPointObject = ap.gameObject;
        }

        if (attackPointObject != null)
            attackPointObject.SetActive(false);

        // Determine initial facing based on scale
        facingRight = transform.localScale.x > 0;
    }

    void Update()
    {
        if (player == null) return;
        
        // Stop jika boss mati
        if (currentState == BossState.Dead || (bossHealth != null && bossHealth.IsDead())) 
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Stop jika player mati
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsDead())
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isRunning", false);
            currentState = BossState.Idle;
            return;
        }

        // Ground check
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool playerInRange = distanceToPlayer <= detectRange && IsPlayerVisible();

        // State machine
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdle(playerInRange, distanceToPlayer);
                break;

            case BossState.Chase:
                HandleChase(distanceToPlayer);
                break;

            case BossState.Retreat:
                HandleRetreat();
                break;

            case BossState.Jump:
                HandleJump();
                break;

            case BossState.Attack1:
            case BossState.Attack2:
                HandleAttack();
                break;

            case BossState.Hurt:
                HandleHurt();
                break;
        }
    }

    bool IsPlayerVisible()
    {
        // Cek apakah player dalam view camera
        Camera cam = Camera.main;
        if (cam == null) return true;

        Vector3 viewPos = cam.WorldToViewportPoint(player.position);
        return viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1;
    }

    void HandleIdle(bool playerInRange, float distance)
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("isRunning", false);

        if (playerInRange)
        {
            ChangeState(BossState.Chase);
        }
    }

    void HandleChase(float distance)
    {
        FacePlayer();


        // Terlalu dekat - retreat atau attack
        if (distance <= retreatRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isRunning", false);

            if (CanAttack())
            {
                ChooseAttack();
            }
            else if (Random.value < 0.3f)
            {
                ChangeState(BossState.Retreat);
            }
            return;
        }

        // Dalam attack range - attack!
        if (distance <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isRunning", false);

            if (CanAttack())
            {
                ChooseAttack();
            }
            return;
        }

        // Di luar attack range - chase player
        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);
        anim.SetBool("isRunning", true);

        // Keluar detect range - kembali idle
        if (distance > detectRange || !IsPlayerVisible())
        {
            ChangeState(BossState.Idle);
        }
    }

    void HandleRetreat()
    {
        stateTimer -= Time.deltaTime;

        // Mundur dari player
        float dir = facingRight ? -1f : 1f; // Mundur = kebalikan facing
        rb.linearVelocity = new Vector2(dir * retreatSpeed, rb.linearVelocity.y);
        anim.SetBool("isRunning", true);

        if (stateTimer <= 0)
        {
            ChangeState(BossState.Chase);
        }
    }

    void HandleJump()
    {
        // Jump dilakukan sekali saat masuk state
        // Setelah landing, kembali chase
        if (isGrounded && rb.linearVelocity.y <= 0)
        {
            ChangeState(BossState.Chase);
        }
    }

    void HandleAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        // Attack state dihandle oleh animation event (OnAttackFinished)
    }

    void HandleHurt()
    {
        stateTimer -= Time.deltaTime;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (stateTimer <= 0)
        {
            ChangeState(BossState.Chase);
        }
    }

    bool CanAttack()
    {
        return Time.time > lastAttackTime + attackCooldown;
    }

    void ChooseAttack()
    {
        FacePlayer();
        
        string attack = Random.value > 0.5f ? "att1" : "att2";
        
        anim.ResetTrigger("att1");
        anim.ResetTrigger("att2");
        anim.SetTrigger(attack);
        
        lastAttackTime = Time.time;
        currentState = attack == "att1" ? BossState.Attack1 : BossState.Attack2;
    }

    void FacePlayer()
    {
        if (player == null) return;

        float dir = player.position.x - transform.position.x;
        
        if (dir > 0.1f && !facingRight)
            Flip();
        else if (dir < -0.1f && facingRight)
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void ChangeState(BossState newState)
    {
        // Exit current state
        anim.SetBool("isRunning", false);

        currentState = newState;

        // Enter new state
        switch (newState)
        {
            case BossState.Idle:
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                break;

            case BossState.Chase:
                break;

            case BossState.Retreat:
                stateTimer = retreatDuration;
                break;

            case BossState.Jump:
                if (isGrounded)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                    anim.SetTrigger("jump");
                }
                break;

            case BossState.Attack1:
                lastAttackTime = Time.time;
                anim.SetTrigger("att1");
                break;

            case BossState.Attack2:
                lastAttackTime = Time.time;
                anim.SetTrigger("att2");
                break;

            case BossState.Hurt:
                stateTimer = hurtDuration;
                anim.SetTrigger("hit");
                break;

            case BossState.Dead:
                rb.linearVelocity = Vector2.zero;
                anim.SetTrigger("death");
                break;
        }
    }

    // Dipanggil dari BossHealth saat kena damage
    public void OnHurt()
    {
        if (currentState != BossState.Dead)
        {
            ChangeState(BossState.Hurt);
        }
    }

    // Animation Events
    public void EnableAttack()
    {
        if (attackPointObject != null)
        {
            attackPointObject.SetActive(true);
        }
    }

    public void DisableAttack()
    {
        if (attackPointObject != null)
        {
            attackPointObject.SetActive(false);
        }
    }

    public void OnAttackFinished()
    {
        if (currentState == BossState.Attack1 || currentState == BossState.Attack2)
        {
            // Jeda sebentar sebelum chase lagi
            ChangeState(BossState.Idle);
            Invoke(nameof(BackToChase), idleAfterAttack);
        }
    }

    void BackToChase()
    {
        if (currentState == BossState.Idle && !bossHealth.IsDead())
        {
            ChangeState(BossState.Chase);
        }
    }

    // Public method untuk damage (backward compatibility)
    public void TakeDamage(int damage)
    {
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(damage);
        }
    }

    // Gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retreatRange);

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}




