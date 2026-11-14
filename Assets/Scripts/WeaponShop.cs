
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShop : MonoBehaviour
{
    public GameObject shopPanel;
    public ZombieSpawner zombieSpawner;
    public GunShooting gunShooting;

    public int smgPrice = 900;

    private bool playerNearby = false;
    private bool smgPurchased = false;

    void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (shopPanel == null || zombieSpawner == null)
            return;

        // Shop is available only during breaks
        if (!zombieSpawner.IsBreak || !playerNearby)
        {
            shopPanel.SetActive(false);
            return;
        }

        if (Keyboard.current == null)
            return;

        // Open or close shop
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            shopPanel.SetActive(!shopPanel.activeSelf);
        }

        // Close shop with Escape
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            shopPanel.SetActive(false);
        }
    }

    public void BuySMG()
    {
        if (Time.timeScale == 0f)
            return;

        if (!playerNearby || zombieSpawner == null ||
            !zombieSpawner.IsBreak || shopPanel == null ||
            !shopPanel.activeSelf)
            return;

        if (smgPurchased)
        {
            Debug.Log("SMG already purchased!");
            return;
        }

        if (gunShooting == null || PointsManager.Instance == null)
        {
            Debug.LogError("Shop references are missing!");
            return;
        }

        // Check balance and deduct points
        if (!PointsManager.Instance.SpendPoints(smgPrice))
        {
            Debug.Log("Not enough points!");
            return;
        }

        // Equip weapon
        gunShooting.EquipSMG();

        smgPurchased = true;

        Debug.Log("SMG purchased successfully!");

        shopPanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (shopPanel != null)
                shopPanel.SetActive(false);
        }
    }
}
