
using UnityEngine;

public class HellhoundAnimationController : MonoBehaviour
{
    [Header("Animation")]
    public Animator hellhoundAnimator;

    [Header("Attack")]
    public float attackDistance = 1.1f;
    public float attackInterval = 1f;

    [Header("Hurt")]
    public float hurtDuration = 0.5f;

    private Transform player;

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
        if (hellhoundAnimator == null)
        {
            hellhoundAnimator =
                GetComponentInChildren<Animator>();
        }
    }

    void Start()
    {
        GameObject playerObject = GameObject.Find("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (isDead ||
            Time.timeScale == 0f ||
            hellhoundAnimator == null ||
            player == null)
            return;

        if (Time.time < hurtEndTime)
            return;

        Vector2 directionToPlayer =
            player.position - transform.position;

        float distance = directionToPlayer.magnitude;

        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            facingDirection = GetDirection(directionToPlayer);
        }

        // Attack when close to the player
        if (distance <= attackDistance)
        {
            if (Time.time >= nextAttackTime)
            {
                string attackName =
                    "Hellhound_Attack_" + facingDirection;

                PlayAnimation(attackName, true);

                nextAttackTime = Time.time +
                    Mathf.Max(
                        attackInterval,
                        GetAnimationLength(attackName)
                    );
            }

            return;
        }

        // Chase the player
        PlayAnimation(
            "Hellhound_Run_" + facingDirection
        );
    }

    public void PlayHurt()
    {
        if (isDead || hellhoundAnimator == null)
            return;

        string animationName =
            "Hellhound_Hurt_" + facingDirection;

        hurtEndTime = Time.time +
            Mathf.Max(
                hurtDuration,
                GetAnimationLength(animationName)
            );

        PlayAnimation(animationName, true);
    }

    public void PlayDeath()
    {
        if (isDead || hellhoundAnimator == null)
            return;

        isDead = true;

        PlayAnimation(
            "Hellhound_Death_" + facingDirection,
            true
        );
    }

    private string GetDirection(Vector2 vector)
    {
        float angle =
            Mathf.Atan2(vector.y, vector.x) *
            Mathf.Rad2Deg;

        int index = Mathf.RoundToInt(angle / 45f);

        index = (index % 8 + 8) % 8;

        return directions[index];
    }

    private float GetAnimationLength(string animationName)
    {
        if (hellhoundAnimator == null ||
            hellhoundAnimator.runtimeAnimatorController == null)
            return 0.5f;

        foreach (AnimationClip clip in
            hellhoundAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == animationName)
                return clip.length;
        }

        return 0.5f;
    }

    private void PlayAnimation(
        string animationName,
        bool restart = false)
    {
        if (hellhoundAnimator == null)
            return;

        if (!restart && currentAnimation == animationName)
            return;

        int hash = Animator.StringToHash(
            "Base Layer." + animationName
        );

        if (!hellhoundAnimator.HasState(0, hash))
        {
            Debug.LogWarning(
                "Missing Hellhound animation: " + animationName
            );
            return;
        }

        hellhoundAnimator.Play(hash, 0, 0f);
        currentAnimation = animationName;
    }
}
