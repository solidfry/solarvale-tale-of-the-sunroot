using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class UpdatePlayerPrefs : MonoBehaviour
{
    [SerializeField]
    List<PlayerPrefItem> preferences;

   

    public const float DefaultSliderValue = 0.5f;

    private void Awake()
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

            slider.value = PlayerPrefs.GetFloat(Key, DefaultSliderValue);

            OnValueChanged.Invoke(slider.value);

        }
    }
}

