using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth = -1; // -1 = belum init

    [Header("Animation Trigger Names")]
    public string hurtTrigger = "GetHit";
    public string deadTrigger = "Dead";

    private Animator anim;
    private bool isDead = false;

    void Awake()
    {
        if (currentHealth < 0)
        {
            currentHealth = maxHealth;
        }
        anim = GetComponent<Animator>();

        // Pastikan player selalu memakai trigger animasi yang benar
        if (gameObject.CompareTag("Player"))
        {
            if (string.IsNullOrEmpty(hurtTrigger) || hurtTrigger == "GetHit")
            {
                hurtTrigger = "Trigger Hurt";
            }

            if (string.IsNullOrEmpty(deadTrigger) || deadTrigger == "Dead")
            {
                deadTrigger = "Trigger Dead";
            }
        }
    }

    void Start()
    {
        if (currentHealth < 0)
        {
            currentHealth = maxHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else if (anim != null && !string.IsNullOrEmpty(hurtTrigger))
        {
            anim.ResetTrigger(hurtTrigger);
            anim.SetTrigger(hurtTrigger);
        }
    }

    void Die()
    {
        isDead = true;

        if (anim != null)
        {
            if (deadTrigger.Contains("Trigger") || deadTrigger.Contains("trigger"))
            {
                anim.ResetTrigger(deadTrigger);
                anim.SetTrigger(deadTrigger);
            }
            else
            {
                anim.Play(deadTrigger, 0, 0f);
            }
        }

        if (gameObject.CompareTag("Player"))
        {
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
                playerController.enabled = false;

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (GameOverManager.Instance != null && GameOverManager.Instance.deathAnimationDelay > 0f)
            {
                StartCoroutine(TriggerGameOverAfterAnimationFallback());
            }
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void OnPlayerDeathAnimationFinished()
    {
        if (!gameObject.CompareTag("Player"))
            return;

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver();
        }
    }

    private System.Collections.IEnumerator TriggerGameOverAfterAnimationFallback()
    {
        if (GameOverManager.Instance == null)
            yield break;

        float delay = Mathf.Max(0f, GameOverManager.Instance.deathAnimationDelay);
        if (delay <= 0f)
            yield break;

        yield return new WaitForSeconds(delay);

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver();
        }
    }
}

