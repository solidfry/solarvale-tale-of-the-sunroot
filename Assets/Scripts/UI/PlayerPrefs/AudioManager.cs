using UnityEngine;
using UnityEngine.Events;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        UpdatePlayerPrefs playerPrefs = FindObjectOfType<UpdatePlayerPrefs>();
        if (playerPrefs != null)
        {
            playerPrefs.OnMasterVolumeChanged.AddListener(SetMasterVolume);
            playerPrefs.OnMusicVolumeChanged.AddListener(SetMusicVolume);
            playerPrefs.OnSfxVolumeChanged.AddListener(SetSfxVolume);

            // Set initial volumes based on PlayerPrefs
            SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
            SetSfxVolume(PlayerPrefs.GetFloat("SfxVolume", 1f));
        }
    }

    private void OnDisable()
    {
        UpdatePlayerPrefs playerPrefs = FindObjectOfType<UpdatePlayerPrefs>();
        if (playerPrefs != null)
        {
            playerPrefs.OnMasterVolumeChanged.RemoveListener(SetMasterVolume);
            playerPrefs.OnMusicVolumeChanged.RemoveListener(SetMusicVolume);
            playerPrefs.OnSfxVolumeChanged.RemoveListener(SetSfxVolume);
        }
    }

    public void SetMasterVolume(float volume)
    {
        // Store master volume in PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();

        // Apply master volume if needed
        Debug.Log("Master Volume set to: " + volume);
    }

    public void SetMusicVolume(float volume)
    {
        // Store music volume in PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();

        // Apply music volume if needed
        Debug.Log("Music Volume set to: " + volume);
    }

    public void SetSfxVolume(float volume)
    {
        // Store SFX volume in PlayerPrefs
        PlayerPrefs.SetFloat("SfxVolume", volume);
        PlayerPrefs.Save();

        // Apply SFX volume if needed
        Debug.Log("SFX Volume set to: " + volume);
    }
}


