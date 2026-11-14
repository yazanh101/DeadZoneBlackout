
using System.Collections;
using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    [Header("Zombie Health")]
    public int maxHealth = 50;

    [Header("Rewards")]
    public int rewardPoints = 40;

    [Header("Health Drop")]
    public GameObject healthPackPrefab;

    [Range(0f, 1f)]
    public float healthDropChance = 0.15f;

    [Header("Death Animation")]
    public float deathDelay = 0.8f;

    [Header("Enemy Sounds")]
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Range(0f, 1f)]
    public float hitVolume = 0.5f;

    [Range(0f, 1f)]
    public float deathVolume = 0.6f;

    private int currentHealth;
    private bool isDead = false;

    private SkeletonAnimationController skeletonAnimation;
    private HellhoundAnimationController hellhoundAnimation;
    private ZombieAI zombieAI;
    private Rigidbody2D rb;

    void Awake()
    {
        skeletonAnimation =
            GetComponent<SkeletonAnimationController>();

        hellhoundAnimation =
            GetComponent<HellhoundAnimationController>();

        zombieAI = GetComponent<ZombieAI>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = Mathf.Max(1, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            PlaySound(hitSound, hitVolume);

            if (hellhoundAnimation != null)
                hellhoundAnimation.PlayHurt();
            else if (skeletonAnimation != null)
                skeletonAnimation.PlayHurt();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (zombieAI != null)
            zombieAI.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // Play death sound
        PlaySound(deathSound, deathVolume);

        // Reward points
        if (PointsManager.Instance != null)
            PointsManager.Instance.AddPoints(rewardPoints);

        // Random health drop
        if (healthPackPrefab != null &&
            Random.value < healthDropChance)
        {
            Instantiate(
                healthPackPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        // Play death animation
        if (hellhoundAnimation != null &&
            hellhoundAnimation.isActiveAndEnabled)
        {
            hellhoundAnimation.PlayDeath();
            StartCoroutine(DestroyAfterDeath());
        }
        else if (skeletonAnimation != null &&
                 skeletonAnimation.isActiveAndEnabled)
        {
            skeletonAnimation.PlayDeath();
            StartCoroutine(DestroyAfterDeath());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(
            Mathf.Max(0f, deathDelay)
        );

        Destroy(gameObject);
    }

    // Play 2D sound independently from enemy object
    void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        GameObject soundObject =
            new GameObject("EnemySound");

        AudioSource source =
            soundObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = volume;
        source.clip = clip;

        source.Play();

        Destroy(soundObject, clip.length + 0.1f);
    }
}
