
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 15f;
    public float lifetime = 2f;
    public int damage = 25;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Destroy bullet after its lifetime
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector2 direction)
    {
        if (rb == null)
            return;

        rb.linearVelocity = direction.normalized * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore the player
        if (other.CompareTag("Player"))
            return;

        // Destroy bullet when hitting a barricade
        if (other.gameObject.layer ==
            LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject);
            return;
        }

        // Damage zombies and zombie dogs
        ZombieHealth zombie =
            other.GetComponent<ZombieHealth>();

        if (zombie != null)
        {
            zombie.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
