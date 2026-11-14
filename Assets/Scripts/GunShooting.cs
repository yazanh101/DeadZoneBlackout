
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GunShooting : MonoBehaviour
{
    public Bullet bulletPrefab;

    [Header("Weapons")]
    public int pistolDamage = 25;
    public int smgDamage = 14;
    public float smgFireRate = 7f;

    [Header("Ammo")]
    public int pistolMagazineSize = 12;
    public int smgMagazineSize = 30;
    public float reloadTime = 1.5f;
    public TMP_Text ammoText;

    [Header("Soldier Animation")]
    public SoldierAnimationController soldierAnimation;

    [Header("Weapon Sounds")]
    public AudioSource gunAudioSource;
    public AudioClip pistolShotSound;
    public AudioClip smgShotSound;
    public AudioClip reloadSound;

    [Header("Sound Volume")]
    [Range(0f, 1f)]
    public float shootingVolume = 0.8f;

    [Range(0f, 1f)]
    public float reloadVolume = 0.7f;

    private bool usingSMG = false;
    private bool isReloading = false;

    private int currentAmmo;
    private int magazineSize;
    private float nextShotTime = 0f;

    void Start()
    {
        // Find soldier animation controller
        if (soldierAnimation == null)
        {
            soldierAnimation =
                GetComponentInParent<SoldierAnimationController>();
        }

        // Find Audio Source automatically
        if (gunAudioSource == null)
        {
            gunAudioSource =
                GetComponent<AudioSource>();
        }

        // Start with pistol
        magazineSize = pistolMagazineSize;
        currentAmmo = magazineSize;

        UpdateAmmoUI();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        // Press R to reload
        if (Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame &&
            !isReloading &&
            currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
        }

        // Cannot shoot while reloading
        if (isReloading)
            return;

        if (Mouse.current == null || bulletPrefab == null)
            return;

        // Pistol: single shot
        // SMG: automatic fire
        bool wantsToShoot = usingSMG
            ? Mouse.current.leftButton.isPressed
            : Mouse.current.leftButton.wasPressedThisFrame;

        if (!wantsToShoot)
            return;

        // Cannot shoot without ammo
        if (currentAmmo <= 0)
            return;

        if (usingSMG)
        {
            // SMG fire rate control
            if (Time.time >= nextShotTime)
            {
                Shoot(smgDamage);

                nextShotTime =
                    Time.time + 1f / Mathf.Max(0.1f, smgFireRate);
            }
        }
        else
        {
            Shoot(pistolDamage);
        }
    }

    // Fire a bullet
    void Shoot(int damage)
    {
        Vector3 spawnPosition =
            transform.position + transform.up * 0.4f;

        Bullet bullet = Instantiate(
            bulletPrefab,
            spawnPosition,
            transform.rotation
        );

        bullet.damage = damage;
        bullet.Launch(transform.up);

        // Remove one bullet
        currentAmmo--;

        UpdateAmmoUI();

        // Play shooting sound
        if (gunAudioSource != null)
        {
            AudioClip shotSound = usingSMG
                ? smgShotSound
                : pistolShotSound;

            if (shotSound != null)
            {
                gunAudioSource.PlayOneShot(
                    shotSound,
                    shootingVolume
                );
            }
        }

        // Play shooting animation
        if (soldierAnimation != null)
        {
            Vector2 aimDirection = transform.up;

            soldierAnimation.PlayShoot(aimDirection);
        }
    }

    // Reload weapon
    IEnumerator Reload()
    {
        isReloading = true;

        UpdateAmmoUI();

        // Play reload sound
        if (gunAudioSource != null &&
            reloadSound != null)
        {
            gunAudioSource.PlayOneShot(
                reloadSound,
                reloadVolume
            );
        }

        // Start reload animation
        if (soldierAnimation != null)
        {
            Vector2 aimDirection = transform.up;

            soldierAnimation.PlayReload(
                aimDirection,
                reloadTime
            );
        }

        // Wait until reloading finishes
        yield return new WaitForSeconds(reloadTime);

        // Refill magazine
        currentAmmo = magazineSize;

        isReloading = false;

        UpdateAmmoUI();
    }

    // Switch from pistol to SMG
    public void EquipSMG()
    {
        StopAllCoroutines();

        isReloading = false;
        usingSMG = true;

        magazineSize = smgMagazineSize;
        currentAmmo = magazineSize;

        nextShotTime = 0f;

        UpdateAmmoUI();

        Debug.Log("SMG EQUIPPED!");
    }

    // Update ammunition display
    void UpdateAmmoUI()
    {
        if (ammoText == null)
            return;

        if (isReloading)
        {
            ammoText.text = "RELOADING...";
        }
        else
        {
            ammoText.text =
                currentAmmo + " / " + magazineSize;
        }
    }
}
