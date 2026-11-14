
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Header("Health Pack Settings")]
    public float healAmount = 30f;
    public float lifetime = 15f;

    void Start()
    {
        // Disappear after 15 seconds
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        // Restore player health
        playerHealth.Heal(healAmount);

        Debug.Log("HEALTH PACK COLLECTED! +30 HP");

        // Remove the health pack
        Destroy(gameObject);
    }
}
