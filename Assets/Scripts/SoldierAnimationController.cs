
using UnityEngine;

public class SoldierAnimationController : MonoBehaviour
{
    public Animator soldierAnimator;

    [Header("Shooting Animation")]
    public float shootDuration = 0.5f;
    public float shootAnimationSpeed = 2f;

    [Header("Reload Animation")]
    public float reloadDuration = 1.5f;

    [Header("Hurt Animation")]
    public float hurtDuration = 0.6f;

    private Rigidbody2D rb;

    private string currentAnimation = "";
    private string facingDirection = "S";
    private string reloadDirection = "S";

    private float shootEndTime = 0f;
    private float reloadEndTime = 0f;
    private float reloadStartTime = 0f;
    private float activeReloadDuration = 1.5f;
    private float hurtEndTime = 0f;

    private bool isDead = false;

    private readonly string[] directions =
    {
        "E", "NE", "N", "NW",
        "W", "SW", "S", "SE"
    };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (soldierAnimator == null)
        {
            soldierAnimator =
                GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (isDead || rb == null ||
            soldierAnimator == null ||
            Time.timeScale == 0f)
            return;

        // Hurt animation has visual priority
        if (Time.time < hurtEndTime)
            return;

        // Resume reload after hurt
        if (Time.time < reloadEndTime)
        {
            string reloadAnimation =
                "Soldier_Reload_" + reloadDirection;

            if (currentAnimation != reloadAnimation)
            {
                float progress = Mathf.Clamp01(
                    (Time.time - reloadStartTime) /
                    Mathf.Max(0.01f, activeReloadDuration)
                );

                PlayAnimation(reloadAnimation, progress);
            }

            return;
        }

        // Keep shooting animation active
        if (Time.time < shootEndTime)
            return;

        soldierAnimator.speed = 1f;

        Vector2 velocity = rb.linearVelocity;

        bool isMoving =
            velocity.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            facingDirection = GetDirection(velocity);
        }

        string animationName = isMoving
            ? "Soldier_Walk_" + facingDirection
            : "Soldier_Idle_" + facingDirection;

        PlayAnimation(animationName);
    }

    // Shooting animation
    public void PlayShoot(Vector2 aimDirection)
    {
        if (isDead ||
            soldierAnimator == null ||
            Time.timeScale == 0f ||
            Time.time < reloadEndTime ||
            Time.time < hurtEndTime)
            return;

        if (aimDirection.sqrMagnitude > 0.001f)
        {
            facingDirection = GetDirection(aimDirection);
        }

        string animationName =
            "Soldier_Shoot_" + facingDirection;

        int stateHash = Animator.StringToHash(
            "Base Layer." + animationName
        );

        if (!soldierAnimator.HasState(0, stateHash))
        {
            Debug.LogWarning(
                "Missing shooting animation: " + animationName
            );
            return;
        }

        bool restartAnimation =
            currentAnimation != animationName;

        if (!restartAnimation)
        {
            AnimatorStateInfo state =
                soldierAnimator.GetCurrentAnimatorStateInfo(0);

            restartAnimation =
                state.normalizedTime >= 0.95f;
        }

        soldierAnimator.speed =
            Mathf.Max(0.01f, shootAnimationSpeed);

        if (restartAnimation)
        {
            soldierAnimator.Play(stateHash, 0, 0f);
            currentAnimation = animationName;
        }

        shootEndTime = Time.time + shootDuration;
    }

    // Reload animation
    public void PlayReload(
        Vector2 aimDirection,
        float duration)
    {
        if (isDead ||
            soldierAnimator == null ||
            Time.timeScale == 0f)
            return;

        if (aimDirection.sqrMagnitude > 0.001f)
        {
            facingDirection = GetDirection(aimDirection);
        }

        reloadDirection = facingDirection;

        activeReloadDuration =
            duration > 0f ? duration : reloadDuration;

        reloadStartTime = Time.time;
        reloadEndTime = Time.time + activeReloadDuration;

        shootEndTime = 0f;

        if (Time.time < hurtEndTime)
            return;

        PlayAnimation(
            "Soldier_Reload_" + reloadDirection
        );
    }

    // Hurt animation
    public void PlayHurt()
    {
        if (isDead ||
            soldierAnimator == null ||
            Time.timeScale == 0f)
            return;

        if (rb != null &&
            Time.time >= reloadEndTime &&
            Time.time >= shootEndTime)
        {
            Vector2 velocity = rb.linearVelocity;

            if (velocity.sqrMagnitude > 0.01f)
            {
                facingDirection = GetDirection(velocity);
            }
        }

        string animationName =
            "Soldier_Hurt_" + facingDirection;

        // Restart animation for every hit
        currentAnimation = "";

        if (PlayAnimation(animationName))
        {
            hurtEndTime = Time.time + hurtDuration;
            shootEndTime = 0f;
        }
    }

    // Death animation
    public void PlayDeath()
    {
        if (isDead)
            return;

        // Keep the current facing direction.
        // If walking, use the last movement direction.
        if (rb != null &&
            Time.time >= shootEndTime &&
            Time.time >= reloadEndTime)
        {
            Vector2 velocity = rb.linearVelocity;

            if (velocity.sqrMagnitude > 0.01f)
            {
                facingDirection = GetDirection(velocity);
            }
        }

        // Lock animation state permanently
        isDead = true;

        // Cancel all previous animation timers
        shootEndTime = 0f;
        reloadEndTime = 0f;
        hurtEndTime = 0f;

        if (soldierAnimator == null)
            return;

        soldierAnimator.speed = 1f;

        string animationName =
            "Soldier_Death_" + facingDirection;

        // Death overrides every other animation
        currentAnimation = "";

        PlayAnimation(animationName);

        Debug.Log(
            "Soldier death animation: " + animationName
        );
    }

    // Convert vector to 8 directions
    private string GetDirection(Vector2 vector)
    {
        float angle =
            Mathf.Atan2(vector.y, vector.x)
            * Mathf.Rad2Deg;

        int index = Mathf.RoundToInt(angle / 45f);

        index = (index % 8 + 8) % 8;

        return directions[index];
    }

    // Play animation without transitions
    private bool PlayAnimation(
        string animationName,
        float normalizedTime = 0f)
    {
        if (soldierAnimator == null)
            return false;

        if (animationName == currentAnimation)
            return true;

        int stateHash = Animator.StringToHash(
            "Base Layer." + animationName
        );

        if (!soldierAnimator.HasState(0, stateHash))
        {
            Debug.LogWarning(
                "Missing animation: " + animationName
            );

            currentAnimation = animationName;
            return false;
        }

        soldierAnimator.speed = 1f;

        soldierAnimator.Play(
            stateHash,
            0,
            normalizedTime
        );

        currentAnimation = animationName;

        return true;
    }
}
