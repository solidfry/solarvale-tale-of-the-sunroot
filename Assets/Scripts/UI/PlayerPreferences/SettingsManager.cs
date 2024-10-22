using UnityEngine;
using UnityEngine.Events;

namespace UI.PlayerPreferences
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField]
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
            if (playerPreferences != null && playerPreferences.Preferences.Count > 0)
            {
                AddVolumeListener("MasterVolume", SetMasterVolume);
                AddVolumeListener("MusicVolume", SetMusicVolume);
                AddVolumeListener("SfxVolume", SetSfxVolume);
                AddVolumeListener("UIVolume", SetSfxVolume);


                // Set initial volumes based on PlayerPrefs
                SetVolume("MasterVolume", PlayerPrefs.GetFloat("MasterVolume", UpdatePlayerPreferences.DefaultSliderValue));
                SetVolume("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", UpdatePlayerPreferences.DefaultSliderValue));
                SetVolume("SfxVolume", PlayerPrefs.GetFloat("SfxVolume", UpdatePlayerPreferences.DefaultSliderValue));
                SetVolume("UIVolume", PlayerPrefs.GetFloat("UIVolume", UpdatePlayerPreferences.DefaultSliderValue));

            }
        }

        private void OnDisable()
        {
            if (playerPreferences != null && playerPreferences.Preferences.Count > 0)
            {
                RemoveVolumeListener("MasterVolume", SetMasterVolume);
                RemoveVolumeListener("MusicVolume", SetMusicVolume);
                RemoveVolumeListener("SfxVolume", SetSfxVolume);
                RemoveVolumeListener("UIVolume", SetSfxVolume);
            }
        }

        private void AddVolumeListener(string key, UnityAction<float> listener)
        {
            var preference = playerPreferences.FindValue(key);
            if (preference != null)
            {
                preference.onValueChanged.AddListener(listener);
            }
        }

        private void RemoveVolumeListener(string key, UnityAction<float> listener)
        {
            var preference = playerPreferences.FindValue(key);
            if (preference != null)
            {
                preference.onValueChanged.RemoveListener(listener);
            }
        }

        private void SetVolume(string key, float volume)
        {
            PlayerPrefs.SetFloat(key, volume);
            PlayerPrefs.Save();

            var rtpcValue = volume * 100;
            AkSoundEngine.SetRTPCValue(key + "Control", rtpcValue);
        }

        public void SetMasterVolume(float volume)
        {
            SetVolume("MasterVolume", volume);
        }

        public void SetMusicVolume(float volume)
        {
            SetVolume("MusicVolume", volume);
        }

        public void SetSfxVolume(float volume)
        {
            SetVolume("SfxVolume", volume);
        }

        public void SetUIVolume(float volume)
        {
            SetVolume("UIVolume", volume);
        }
    }
}
