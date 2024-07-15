using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip musicClip;
    public AudioClip sfxClip;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    public static AudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource = gameObject.AddComponent<AudioSource>();

            musicSource.clip = musicClip;
            sfxSource.clip = sfxClip;

            musicSource.loop = true;
            musicSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        var playerPrefs = FindObjectOfType<UpdatePlayerPrefs>();
        if (playerPrefs != null)
        {
            playerPrefs.OnMasterVolumeChanged.AddListener(SetMasterVolume);
            playerPrefs.OnMusicVolumeChanged.AddListener(SetMusicVolume);
            playerPrefs.OnSfxVolumeChanged.AddListener(SetSfxVolume);
        }
    }

    private void OnDisable()
    {
        var playerPrefs = FindObjectOfType<UpdatePlayerPrefs>();
        if (playerPrefs != null)
        {
            playerPrefs.OnMasterVolumeChanged.RemoveListener(SetMasterVolume);
            playerPrefs.OnMusicVolumeChanged.RemoveListener(SetMusicVolume);
            playerPrefs.OnSfxVolumeChanged.RemoveListener(SetSfxVolume);
        }
    }

    public void SetMasterVolume(float volume)
    {
        musicSource.volume = volume;
        sfxSource.volume = volume;
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
