using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PlayerPreferences
{
    public class UpdatePlayerPreferences : MonoBehaviour
    {
        [SerializeField]
        List<PlayerPrefItem> preferences;
        public List<PlayerPrefItem> Preferences => preferences;

   

        public const float DefaultSliderValue = 0.5f;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            foreach (var item in preferences)
            {
                item.Initialise();
            }
        }

        public PlayerPrefItem FindValue(string key)
        {
            var value = preferences.Find(p => p.Key == key);

            if (value == null)
            {
                return null;
            }

            return value;
        
        }

        public static void SaveVolume(string key, float value, UnityEvent<float> volumeChangedEvent)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
            volumeChangedEvent.Invoke(value);
        }

        [Serializable]
        public class PlayerPrefItem
        {
            public string Key;
            public Slider slider;
            public UnityEvent<float> OnValueChanged = new();

            public void Initialise()
            {
                if (Key == String.Empty || slider == null)
                {
                    Debug.LogError("Slider or Key is empty");

                    return;
                }

                slider.onValueChanged.AddListener(value => SaveVolume(Key, value, OnValueChanged));

                if (!PlayerPrefs.HasKey(Key))
                {
                    PlayerPrefs.SetFloat(Key, DefaultSliderValue);
                }

                var f = PlayerPrefs.GetFloat(Key, DefaultSliderValue);
                slider.value = f;
                OnValueChanged.Invoke(slider.value);

            }
        }
    }
}

