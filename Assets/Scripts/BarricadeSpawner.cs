
using System.Collections.Generic;
using UnityEngine;

public class BarricadeSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject barricadePrefab;
    public Transform player;
    public Transform weaponShop;

    [Header("Map Generation")]
    public int barricadeCount = 8;
    public float mapWidth = 20f;
    public float mapHeight = 11f;

    [Header("Safe Distances")]
    public float playerSafeDistance = 4f;
    public float shopSafeDistance = 4f;
    public float barricadeSpacing = 4.5f;
    public float obstacleClearance = 0.5f;

    private List<GameObject> spawnedBarricades =
        new List<GameObject>();

    public void GenerateBarricades()
    {
        if (barricadePrefab == null ||
            player == null ||
            weaponShop == null)
        {
            Debug.LogError(
                "BarricadeSpawner: Missing references!"
            );
            return;
        }

        int obstacleMask = LayerMask.GetMask("Obstacle");

        if (obstacleMask == 0)
        {
            Debug.LogError("Obstacle layer not found!");
            return;
        }

        BoxCollider2D prefabCollider =
            barricadePrefab.GetComponent<BoxCollider2D>();

        if (prefabCollider == null)
        {
            Debug.LogError(
                "Barricade prefab needs a BoxCollider2D!"
            );
            return;
        }

        // Remove old barricades
        foreach (GameObject barricade in spawnedBarricades)
        {
            if (barricade != null)
            {
                barricade.SetActive(false);
                Destroy(barricade);
            }
        }

        spawnedBarricades.Clear();

        List<Vector2> positions = new List<Vector2>();

        int attempts = 0;
        int maxAttempts = 300;

        while (positions.Count < barricadeCount &&
               attempts < maxAttempts)
        {
            attempts++;

            Vector2 position = new Vector2(
                Random.Range(-mapWidth, mapWidth),
                Random.Range(-mapHeight, mapHeight)
            );

            // Keep player area clear
            if (Vector2.Distance(
                position, player.position
            ) < playerSafeDistance)
                continue;

            // Keep shop area clear
            if (Vector2.Distance(
                position, weaponShop.position
            ) < shopSafeDistance)
                continue;

            // Keep barricades separated
            bool tooClose = false;

            foreach (Vector2 existing in positions)
            {
                if (Vector2.Distance(
                    position, existing
                ) < barricadeSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose)
                continue;

            // Random horizontal or vertical rotation
            float rotation =
                Random.value < 0.5f ? 0f : 90f;

            Vector2 colliderSize = new Vector2(
                Mathf.Abs(
                    prefabCollider.size.x *
                    barricadePrefab.transform.localScale.x
                ),
                Mathf.Abs(
                    prefabCollider.size.y *
                    barricadePrefab.transform.localScale.y
                )
            );

            colliderSize +=
                Vector2.one * obstacleClearance * 2f;

            // Check crates, containers, and other obstacles
            Collider2D blocked = Physics2D.OverlapBox(
                position,
                colliderSize,
                rotation,
                obstacleMask
            );

            if (blocked != null)
                continue;

            GameObject barricade = Instantiate(
                barricadePrefab,
                new Vector3(position.x, position.y, 0f),
                Quaternion.Euler(0f, 0f, rotation),
                transform
            );

            spawnedBarricades.Add(barricade);
            positions.Add(position);
        }

        Debug.Log(
            "NEW MAP LAYOUT: " +
            spawnedBarricades.Count +
            " barricades generated!"
        );

        if (spawnedBarricades.Count < barricadeCount)
        {
            Debug.LogWarning(
                "Not enough free space for all barricades."
            );
        }
    }
}
