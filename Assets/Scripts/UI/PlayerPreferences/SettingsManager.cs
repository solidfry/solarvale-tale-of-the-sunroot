using UI.PlayerPreferences;
using UnityEngine;
using UnityEngine.Serialization;

public class SettingsManager : MonoBehaviour
{
    [FormerlySerializedAs("playerPrefs")] [SerializeField]
    private UpdatePlayerPreferences playerPreferences;

    private void Start()
    {
        if (playerPreferences == null)
        {
            playerPreferences = GetComponent<UpdatePlayerPreferences>();
        }
    }

    private void OnEnable()
    {
        if (playerPreferences != null)
        {
            playerPreferences.FindValue("MasterVolume").OnValueChanged.AddListener(SetMasterVolume);
            playerPreferences.FindValue("MusicVolume").OnValueChanged.AddListener(SetMusicVolume);
            playerPreferences.FindValue("SfxVolume").OnValueChanged.AddListener(SetSfxVolume);

            // Set initial volumes based on PlayerPrefs
            SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", UpdatePlayerPreferences.DefaultSliderValue));
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", UpdatePlayerPreferences.DefaultSliderValue));
            SetSfxVolume(PlayerPrefs.GetFloat("SfxVolume", UpdatePlayerPreferences.DefaultSliderValue));
        }
    }

    private void OnDisable()
    {
        if (playerPreferences != null)
        {
            playerPreferences.FindValue("MasterVolume").OnValueChanged.RemoveListener(SetMasterVolume);
            playerPreferences.FindValue("MusicVolume").OnValueChanged.RemoveListener(SetMasterVolume);
            playerPreferences.FindValue("SfxVolume").OnValueChanged.RemoveListener(SetMasterVolume);
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

