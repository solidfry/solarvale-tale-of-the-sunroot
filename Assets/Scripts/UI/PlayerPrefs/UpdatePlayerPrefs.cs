using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpdatePlayerPrefs : MonoBehaviour
{
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    [Space(20)]
    public UnityEvent<float> OnMasterVolumeChanged = new UnityEvent<float>();
    public UnityEvent<float> OnMusicVolumeChanged = new UnityEvent<float>();
    public UnityEvent<float> OnSfxVolumeChanged = new UnityEvent<float>();

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";

    private void Awake()
    { 
        // Add listeners to sliders to save volume and invoke corresponding events when changed
        masterVolumeSlider.onValueChanged.AddListener(value => SaveVolume(MasterVolumeKey, value, OnMasterVolumeChanged));
        musicVolumeSlider.onValueChanged.AddListener(value => SaveVolume(MusicVolumeKey, value, OnMusicVolumeChanged));
        sfxVolumeSlider.onValueChanged.AddListener(value => SaveVolume(SfxVolumeKey, value, OnSfxVolumeChanged));

        // Load saved volume values from PlayerPrefs, defaulting to 1f if not set
        masterVolumeSlider.value = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);

        // Invoke events with initial values to set initial volume levels
        OnMasterVolumeChanged.Invoke(masterVolumeSlider.value);
        OnMusicVolumeChanged.Invoke(musicVolumeSlider.value);
        OnSfxVolumeChanged.Invoke(sfxVolumeSlider.value);

    }

    private void SaveVolume(string key, float value, UnityEvent<float> volumeChangedEvent)
    {
        // Save the volume value to PlayerPrefs and invoke the corresponding event
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
        volumeChangedEvent?.Invoke(value);
    }
}

