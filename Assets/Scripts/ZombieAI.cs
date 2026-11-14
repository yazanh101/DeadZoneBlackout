
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ZombieAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float stopDistance = 1f;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1f;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayer;
    public float obstacleDetectionDistance = 1.5f;
    public float obstacleCheckRadius = 0.3f;

    private Rigidbody2D rb;
    private Transform player;
    private PlayerHealth playerHealth;

    private float attackTimer = 0f;

    private Collider2D currentObstacle;
    private Vector2 bypassDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GameObject playerObject =
            GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            playerHealth =
                playerObject.GetComponent<PlayerHealth>();
        }
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        attackTimer -= Time.fixedDeltaTime;

        Vector2 target = player.position;
        Vector2 toPlayer = target - rb.position;

        float distance = toPlayer.magnitude;

        if (distance < 0.001f)
            return;

        Vector2 directionToPlayer =
            toPlayer.normalized;

        // Check if a barricade is between zombie and player
        RaycastHit2D wallBetween =
            Physics2D.Linecast(
                rb.position,
                target,
                obstacleLayer
            );

        // Attack only if the player is close
        // and there is no barricade between them
        if (distance <= stopDistance &&
            wallBetween.collider == null)
        {
            if (attackTimer <= 0f &&
                playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);

                attackTimer = attackCooldown;
            }

            currentObstacle = null;
            return;
        }

        Vector2 moveDirection = directionToPlayer;

        // Look for obstacles in front of the zombie
        RaycastHit2D obstacleHit =
            Physics2D.CircleCast(
                rb.position,
                obstacleCheckRadius,
                directionToPlayer,
                Mathf.Min(
                    distance,
                    obstacleDetectionDistance
                ),
                obstacleLayer
            );

        if (obstacleHit.collider != null)
        {
            // Choose a direction when encountering
            // a new obstacle
            if (currentObstacle != obstacleHit.collider)
            {
                currentObstacle = obstacleHit.collider;

                ChooseBypassDirection(
                    obstacleHit.collider
                );
            }

            // Move along the obstacle toward its edge
            moveDirection = bypassDirection;
        }
        else
        {
            // The path is clear: chase the player
            currentObstacle = null;
            moveDirection = directionToPlayer;
        }

        Vector2 newPosition =
            rb.position +
            moveDirection *
            moveSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(newPosition);
    }

    void ChooseBypassDirection(Collider2D obstacle)
    {
        Bounds bounds = obstacle.bounds;

        // Horizontal barricade: go left or right
        if (bounds.size.x >= bounds.size.y)
        {
            if (rb.position.x < bounds.center.x)
                bypassDirection = Vector2.left;
            else
                bypassDirection = Vector2.right;
        }
        // Vertical barricade: go up or down
        else
        {
            if (rb.position.y < bounds.center.y)
                bypassDirection = Vector2.down;
            else
                bypassDirection = Vector2.up;
        }
    }
}
