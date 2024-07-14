using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UpdatePlayerPrefs : MonoBehaviour
{
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    public UnityEvent<float> OnMasterVolumeChanged;
    public UnityEvent<float> OnMusicVolumeChanged;
    public UnityEvent<float> OnSfxVolumeChanged;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    private void Start()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        masterVolumeSlider.onValueChanged.AddListener(value => SaveVolume(MasterVolumeKey, value, OnMasterVolumeChanged));
        musicVolumeSlider.onValueChanged.AddListener(value => SaveVolume(MusicVolumeKey, value, OnMusicVolumeChanged));
        sfxVolumeSlider.onValueChanged.AddListener(value => SaveVolume(SfxVolumeKey, value, OnSfxVolumeChanged));

        OnMasterVolumeChanged.Invoke(masterVolumeSlider.value);
        OnMusicVolumeChanged.Invoke(musicVolumeSlider.value);
        OnSfxVolumeChanged.Invoke(sfxVolumeSlider.value);
    }

    private void SaveVolume(string key, float value, UnityEvent<float> volumeChangedEvent)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
        volumeChangedEvent.Invoke(value);
    }
}
