using UnityEngine;

public class KillZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = other.GetComponentInParent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(playerHealth.GetMaxHealth());
            }

            if (GameOverManager.Instance != null)
            {
                GameOverManager.Instance.TriggerGameOverFall();
            }
        }
    }
}

