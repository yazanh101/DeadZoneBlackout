
using UnityEngine;
using UnityEngine.UI;

public class UIClickSound : MonoBehaviour
{
    [Header("UI Sound")]
    public AudioClip clickSound;

    [Range(0f, 1f)]
    public float volume = 0.6f;

    private Button[] buttons;

    void Start()
    {
        // Find all buttons, including hidden panels
        buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            button.onClick.AddListener(PlayClick);
        }
    }

    void PlayClick()
    {
        if (clickSound == null)
            return;

        // Keep playing even when changing scenes
        GameObject soundObject =
            new GameObject("UI Click Sound");

        DontDestroyOnLoad(soundObject);

        AudioSource source =
            soundObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = volume;

        source.PlayOneShot(clickSound);

        Destroy(soundObject, clickSound.length + 0.1f);
    }

    void OnDestroy()
    {
        if (buttons == null)
            return;

        foreach (Button button in buttons)
        {
            if (button != null)
                button.onClick.RemoveListener(PlayClick);
        }
    }
}
