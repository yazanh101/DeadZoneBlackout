
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

public class BlackoutManager : MonoBehaviour
{
    [Header("References")]
    public Light2D globalLight;
    public GameObject flashlight;
    public TMP_Text roundText;
    public ZombieSpawner zombieSpawner;

    [Header("Military Base Lights")]
    public GameObject[] towerLights;

    [Header("Settings")]
    public float blackoutIntensity = 0.05f;
    public bool testBlackout = false;

    private float normalIntensity;
    private bool isBlackout = false;
    private bool[] originalLightStates;

    void Start()
    {
        if (globalLight == null ||
            flashlight == null ||
            roundText == null ||
            zombieSpawner == null)
        {
            Debug.LogError(
                "BlackoutManager: Missing references!"
            );

            enabled = false;
            return;
        }

        normalIntensity = globalLight.intensity;

        flashlight.SetActive(false);

        // Remember which tower lights were enabled
        originalLightStates = new bool[towerLights.Length];

        for (int i = 0; i < towerLights.Length; i++)
        {
            if (towerLights[i] != null)
            {
                originalLightStates[i] =
                    towerLights[i].activeSelf;
            }
        }
    }

    void Update()
    {
        int round = GetCurrentRound();

        bool shouldBlackout =
            testBlackout ||
            (round > 0 &&
             round % 5 == 0 &&
             !zombieSpawner.IsBreak);

        if (shouldBlackout == isBlackout)
            return;

        isBlackout = shouldBlackout;

        // Control global lighting
        globalLight.intensity = isBlackout
            ? blackoutIntensity
            : normalIntensity;

        // Player flashlight
        flashlight.SetActive(isBlackout);

        // Control all military tower lights
        for (int i = 0; i < towerLights.Length; i++)
        {
            if (towerLights[i] == null)
                continue;

            bool shouldBeActive =
                !isBlackout && originalLightStates[i];

            towerLights[i].SetActive(shouldBeActive);
        }

        Debug.Log(
            isBlackout
                ? "BLACKOUT! ALL BASE LIGHTS OFF!"
                : "POWER RESTORED!"
        );
    }

    int GetCurrentRound()
    {
        string text = roundText.text;

        int colon = text.LastIndexOf(':');

        if (colon >= 0 &&
            int.TryParse(
                text.Substring(colon + 1).Trim(),
                out int round))
        {
            return round;
        }

        return 0;
    }
}
