using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private float move;
    private bool isGrounded;
    private bool jumpPressed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Cek apakah player sedang di tanah
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Input horizontal
        move = Input.GetAxisRaw("Horizontal");

        // Jalan
        anim.SetBool("isWalking", move != 0);

        // Lari
        bool runPressed = Input.GetKey(KeyCode.LeftShift);
        anim.SetBool("isRunning", runPressed && move != 0);

        // Input jump (disimpan untuk diproses di FixedUpdate)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }

        // Serangan normal
        if (Input.GetKeyDown(KeyCode.J))
            anim.SetTrigger("Trigger Attack");

        // Spesial Flamejet
        if (Input.GetKeyDown(KeyCode.F))
            anim.SetTrigger("Trigger Flamejet");
    }

    void FixedUpdate()
    {
        // Update ground check di FixedUpdate untuk konsistensi dengan physics
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Movement horizontal
        float speed = anim.GetBool("isRunning") ? runSpeed : walkSpeed;
        float velocityY = rb.linearVelocity.y;

        // Proses jump di FixedUpdate untuk konsistensi dengan physics
        if (jumpPressed && isGrounded)
        {
            velocityY = jumpForce;
            anim.SetTrigger("Trigger Jump");
            jumpPressed = false;
        }
        else if (jumpPressed)
        {
            jumpPressed = false; // Reset jika tidak bisa jump
        }

        // Set velocity (horizontal + vertical)
        rb.linearVelocity = new Vector2(move * speed, velocityY);

        // Flip player
        if (move > 0) transform.localScale = new Vector3(1, 1, 1);
        if (move < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    // Visualisasi GroundCheck
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
