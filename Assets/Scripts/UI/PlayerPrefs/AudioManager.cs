using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioManager : MonoBehaviour
{
    public AK.Wwise.Event musicEvent;
    public AK.Wwise.Event sfxEvent;

    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Start playing the music and SFX events
            musicEvent.Post(gameObject);
            sfxEvent.Post(gameObject);
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
        AkSoundEngine.SetRTPCValue("MasterVolume", volume * 100f);
    }

    public void SetMusicVolume(float volume)
    {
        AkSoundEngine.SetRTPCValue("MusicVolume", volume * 100f);
    }

    public void SetSfxVolume(float volume)
    {
        AkSoundEngine.SetRTPCValue("SfxVolume", volume * 100f);
    }
}

