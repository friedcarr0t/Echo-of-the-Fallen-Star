using UnityEngine;

public class StarFragment : MonoBehaviour
{
    public int chapterIndex; // 0,1,2 untuk chapter 1-3
    public bool isDroppedItem = false; // True jika di-drop dari boss

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (StarManager.Instance != null)
        {
            bool alreadyCollected = chapterIndex >= 0 &&
                                    chapterIndex < StarManager.Instance.collectedStars.Length &&
                                    StarManager.Instance.collectedStars[chapterIndex];

            if (!isDroppedItem && alreadyCollected)
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (StarManager.Instance != null)
            {
                StarManager.Instance.CollectStar(chapterIndex);
            }
            Destroy(gameObject);
        }
    }

    // Untuk collision dengan ground (non-trigger)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Stop movement saat kena ground
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}
