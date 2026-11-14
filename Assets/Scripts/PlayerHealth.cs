
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public Slider healthBar;
    public GameOverManager gameOverManager;

    [Header("Health Regeneration")]
    public float regenDelay = 5f;
    public float regenRate = 3f;

    [Header("Soldier Animation")]
    public SoldierAnimationController soldierAnimation;

    [Header("Death")]
    public float gameOverDelay = 1.1f;

    private float currentHealth;
    private float lastDamageTime;
    private bool isDead = false;

    private PlayerMovement playerMovement;
    private GunShooting gunShooting;
    private Rigidbody2D rb;

    void Awake()
    {
        if (soldierAnimation == null)
        {
            soldierAnimation =
                GetComponent<SoldierAnimationController>();
        }

        playerMovement = GetComponent<PlayerMovement>();

        gunShooting =
            GetComponentInChildren<GunShooting>(true);

        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        lastDamageTime = Time.time;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Update()
    {
        if (isDead || Time.timeScale == 0f)
            return;

        // Wait before health regeneration
        if (Time.time - lastDamageTime < regenDelay)
            return;

        if (currentHealth < maxHealth)
        {
            currentHealth += regenRate * Time.deltaTime;

            currentHealth =
                Mathf.Min(currentHealth, maxHealth);

            UpdateHealthBar();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth -= damage;

        currentHealth =
            Mathf.Max(0, currentHealth);

        lastDamageTime = Time.time;

        UpdateHealthBar();

        Debug.Log(
            "Player HP: " + Mathf.CeilToInt(currentHealth)
        );

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (soldierAnimation != null)
            {
                soldierAnimation.PlayHurt();
            }
        }
    }

    // Called when health reaches zero
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("PLAYER DIED!");

        // Stop player movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Stop shooting and reloading input
        if (gunShooting != null)
        {
            gunShooting.enabled = false;
        }

        // Stop physics movement and collisions
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Start death animation
        if (soldierAnimation != null)
        {
            soldierAnimation.PlayDeath();
        }

        // Show Game Over after the animation
        StartCoroutine(ShowGameOverAfterDeath());
    }

    private IEnumerator ShowGameOverAfterDeath()
    {
        yield return new WaitForSeconds(
            Mathf.Max(0f, gameOverDelay)
        );

        Debug.Log("GAME OVER!");

        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }
    }

    // Health pickups
    public void Heal(float amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth = Mathf.Min(
            currentHealth + amount,
            maxHealth
        );

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }
}
