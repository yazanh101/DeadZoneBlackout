
using System.Collections;
using UnityEngine;
using TMPro;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]


    [Header("Map Generation")]
    public BarricadeSpawner barricadeSpawner;

    public GameObject zombiePrefab;
    public GameObject zombieDogPrefab;

    [Header("UI")]
    public TMP_Text roundText;
    public TMP_Text breakText;

    [Header("Wave Settings")]
    public int totalZombies = 10;
    public float spawnInterval = 2f;
    public float breakDuration = 30f;

    [Header("Difficulty Settings")]
    public int extraZombiesPerRound = 4;
    public int extraHealthPerRound = 4;
    public float extraSpeedPerRound = 0.08f;
    public float maxSpeedBonus = 1.2f;

    public float spawnIntervalReduction = 0.1f;
    public float minimumSpawnInterval = 0.6f;

    [Header("Blackout Settings")]
    public int blackoutEveryRounds = 5;
    public int blackoutBonusPoints = 250;

    [Header("Testing")]
    public int startingRound = 1;

    public bool IsBreak { get; private set; }

    private int currentRound;

    void Start()
    {
        currentRound = Mathf.Max(1, startingRound);
        IsBreak = false;

        if (breakText != null)
        {
            breakText.gameObject.SetActive(false);
        }

        StartCoroutine(RoundLoop());
    }

    IEnumerator RoundLoop()
    {
        while (true)
        {
            // Start a new round
            IsBreak = false;


            // Generate new barricades at the start of each round
            if (barricadeSpawner != null)
            {
                barricadeSpawner.GenerateBarricades();
            }


            bool isBlackoutRound =
                blackoutEveryRounds > 0 &&
                currentRound % blackoutEveryRounds == 0;

            int zombieCount =
                totalZombies +
                (currentRound - 1) * extraZombiesPerRound;

            float currentSpawnInterval = Mathf.Max(
                minimumSpawnInterval,
                spawnInterval -
                (currentRound - 1) * spawnIntervalReduction
            );

            if (roundText != null)
            {
                roundText.text = "ROUND: " + currentRound;
            }

            Debug.Log("ROUND " + currentRound + " STARTED!");
            Debug.Log("Zombies: " + zombieCount);
            Debug.Log("Spawn interval: " +
                currentSpawnInterval);

            if (isBlackoutRound)
            {
                Debug.Log("BLACKOUT WAVE!");
            }

            // Spawn enemies gradually
            for (int i = 0; i < zombieCount; i++)
            {
                bool spawnDog =
                    isBlackoutRound &&
                    zombieDogPrefab != null &&
                    i % 2 == 0;

                SpawnEnemy(spawnDog);

                yield return new WaitForSeconds(
                    currentSpawnInterval
                );
            }

            // Wait until all enemies are defeated
            while (FindObjectsByType<ZombieHealth>(
                FindObjectsSortMode.None).Length > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }

            Debug.Log("ROUND " + currentRound + " COMPLETED!");

            // Bonus for surviving a blackout round
            if (isBlackoutRound &&
                PointsManager.Instance != null)
            {
                PointsManager.Instance.AddPoints(
                    blackoutBonusPoints
                );

                Debug.Log("BLACKOUT BONUS: +" +
                    blackoutBonusPoints + " POINTS!");
            }

            // Start the shopping break
            IsBreak = true;

            if (breakText != null)
            {
                breakText.gameObject.SetActive(true);
            }

            float timeLeft = breakDuration;

            while (timeLeft > 0f)
            {
                if (breakText != null)
                {
                    breakText.text = "BREAK: " +
                        Mathf.CeilToInt(timeLeft);
                }

                yield return null;
                timeLeft -= Time.deltaTime;
            }

            if (breakText != null)
            {
                breakText.gameObject.SetActive(false);
            }

            // Move to next round
            currentRound++;
        }
    }

    void SpawnEnemy(bool spawnDog)
    {
        GameObject prefab = zombiePrefab;

        if (spawnDog && zombieDogPrefab != null)
        {
            prefab = zombieDogPrefab;
        }

        if (prefab == null)
        {
            Debug.LogError("Enemy prefab is missing!");
            return;
        }

        Vector3 spawnPosition;

        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0:
                spawnPosition = new Vector3(
                    -23f,
                    Random.Range(-13f, 13f),
                    0f
                );
                break;

            case 1:
                spawnPosition = new Vector3(
                    23f,
                    Random.Range(-13f, 13f),
                    0f
                );
                break;

            case 2:
                spawnPosition = new Vector3(
                    Random.Range(-23f, 23f),
                    13f,
                    0f
                );
                break;

            default:
                spawnPosition = new Vector3(
                    Random.Range(-23f, 23f),
                    -13f,
                    0f
                );
                break;
        }

        GameObject enemy = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        // Increase health based on the round
        ZombieHealth health =
            enemy.GetComponent<ZombieHealth>();

        if (health != null)
        {
            health.maxHealth +=
                (currentRound - 1) * extraHealthPerRound;
        }

        // Increase movement speed based on the round
        ZombieAI ai =
            enemy.GetComponent<ZombieAI>();

        if (ai != null)
        {
            float speedBonus = Mathf.Min(
                (currentRound - 1) * extraSpeedPerRound,
                maxSpeedBonus
            );

            ai.moveSpeed += speedBonus;
        }
    }
}
