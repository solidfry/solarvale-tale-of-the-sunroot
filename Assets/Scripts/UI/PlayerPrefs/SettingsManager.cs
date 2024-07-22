using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [SerializeField]
    private UpdatePlayerPrefs playerPrefs;

    private void Start()
    {
        if (playerPrefs == null)
        {
            playerPrefs = GetComponent<UpdatePlayerPrefs>();
        }
    }

    private void OnEnable()
    {
        if (playerPrefs != null)
        {
            playerPrefs.FindValue("MasterVolume").OnValueChanged.AddListener(SetMasterVolume);
            playerPrefs.FindValue("MusicVolume").OnValueChanged.AddListener(SetMusicVolume);
            playerPrefs.FindValue("SfxVolume").OnValueChanged.AddListener(SetSfxVolume);

            // Set initial volumes based on PlayerPrefs
            SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", UpdatePlayerPrefs.DefaultSliderValue));
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", UpdatePlayerPrefs.DefaultSliderValue));
            SetSfxVolume(PlayerPrefs.GetFloat("SfxVolume", UpdatePlayerPrefs.DefaultSliderValue));
        }
    }

    private void OnDisable()
    {
        if (playerPrefs != null)
        {
            playerPrefs.FindValue("MasterVolume").OnValueChanged.RemoveListener(SetMasterVolume);
            playerPrefs.FindValue("MusicVolume").OnValueChanged.RemoveListener(SetMasterVolume);
            playerPrefs.FindValue("SfxVolume").OnValueChanged.RemoveListener(SetMasterVolume);
        }
    }

    public void SetMasterVolume(float volume)
    {
        // Store master volume in PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();

        // Apply master volume using Wwise
        float rtpcValue = Mathf.Lerp(-200, 0, volume); // Scale from 0 to 1
        AkSoundEngine.SetRTPCValue("MasterVolumeControl", rtpcValue);
    }

    public void SetMusicVolume(float volume)
    {
        // Store music volume in PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();

        // Apply music volume using Wwise
        float rtpcValue = Mathf.Lerp(-200, 0, volume); // Scale from 0 to 1
        AkSoundEngine.SetRTPCValue("MusicVolume", rtpcValue);
    }

    public void SetSfxVolume(float volume)
    {
        // Store SFX volume in PlayerPrefs
        PlayerPrefs.SetFloat("SfxVolume", volume);
        PlayerPrefs.Save();

        // Apply SFX volume using Wwise
        float rtpcValue = Mathf.Lerp(-200, 0, volume); // Scale from 0 to 1
        AkSoundEngine.SetRTPCValue("SfxVolume", rtpcValue);
    }

   
}

