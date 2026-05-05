using UnityEngine;

public class DestroyOnDeath : MonoBehaviour
{
    public float destroyDelay = 1.0f;
    public bool disableColliders = true;
    public bool freezeRigidbodyOnDeath = true;

    private Health health;
    private bool scheduled = false;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (scheduled) return;
        if (health == null) return;
        if (!health.IsDead()) return;

        scheduled = true;

        if (disableColliders)
        {
            foreach (var c in GetComponentsInChildren<Collider2D>())
                c.enabled = false;
        }

        if (freezeRigidbodyOnDeath)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
        }

        Destroy(gameObject, destroyDelay);
    }
}


