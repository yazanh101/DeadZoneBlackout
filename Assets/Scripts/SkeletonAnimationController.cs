
using UnityEngine;

public class SkeletonAnimationController : MonoBehaviour
{
    [Header("Animation")]
    public Animator skeletonAnimator;

    [Header("Attack")]
    public float attackDistance = 1.1f;
    public float attackInterval = 1f;

    [Header("Hurt")]
    public float hurtDuration = 0.5f;

    private Transform player;
    private Rigidbody2D rb;

    private string currentAnimation = "";
    private string facingDirection = "S";

    private float nextAttackTime;
    private float hurtEndTime;
    private bool isDead = false;

    private readonly string[] directions =
    {
        "E", "NE", "N", "NW",
        "W", "SW", "S", "SE"
    };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (skeletonAnimator == null)
        {
            skeletonAnimator =
                GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        GameObject playerObject =
            GameObject.Find("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (isDead || Time.timeScale == 0f ||
            skeletonAnimator == null)
            return;

        if (Time.time < hurtEndTime)
            return;

        if (player == null)
            return;

        Vector2 directionToPlayer =
            player.position - transform.position;

        float distance = directionToPlayer.magnitude;

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            facingDirection =
                GetDirection(directionToPlayer);
        }

        // Attack when close to the player
        if (distance <= attackDistance)
        {
            if (Time.time >= nextAttackTime)
            {
                PlayAnimation(
                    "Skeleton_Attack_" + facingDirection,
                    true
                );

                nextAttackTime =
                    Time.time + attackInterval;
            }

            return;
        }

        // Walking animation
        PlayAnimation(
            "Skeleton_Walk_" + facingDirection
        );
    }

    public void PlayHurt()
    {
        if (isDead || skeletonAnimator == null)
            return;

        hurtEndTime = Time.time + hurtDuration;

        PlayAnimation(
            "Skeleton_Hurt_" + facingDirection,
            true
        );
    }

    public void PlayDeath()
    {
        if (isDead || skeletonAnimator == null)
            return;

        isDead = true;

        PlayAnimation(
            "Skeleton_Death_" + facingDirection,
            true
        );
    }

    private string GetDirection(Vector2 vector)
    {
        float angle =
            Mathf.Atan2(vector.y, vector.x) *
            Mathf.Rad2Deg;

        int index =
            Mathf.RoundToInt(angle / 45f);

        index = (index % 8 + 8) % 8;

        return directions[index];
    }

    private void PlayAnimation(
        string animationName,
        bool restart = false)
    {
        if (!restart &&
            currentAnimation == animationName)
            return;

        int hash = Animator.StringToHash(
            "Base Layer." + animationName
        );

        if (!skeletonAnimator.HasState(0, hash))
            return;

        skeletonAnimator.Play(hash, 0, 0f);

        currentAnimation = animationName;
    }
}

