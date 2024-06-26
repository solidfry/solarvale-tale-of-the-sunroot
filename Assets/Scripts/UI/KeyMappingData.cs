using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    [CreateAssetMenu(fileName = "KeyMapping", menuName = "UI/KeyMappingData", order = 0)]
    public class KeyMappingData : ScriptableObject
    {
        [SerializeField] InputActionAsset inputActionAsset;
        [SerializeField] List<DeviceMap> deviceMaps;
        [SerializeField] List<KeyMap> keyMap;
        
        public Sprite GetSprite(KeyType keyType)
        {
            foreach (var key in keyMap)
            {
                if (key.keyType == keyType)
                {
                    return key.sprite;
                }
            }
            return null;
        }
        
        [ContextMenu("PopulateDeviceMaps")]
        public void PopulateDeviceMaps()
        {
            var devices = inputActionAsset.devices;
            foreach (var device in devices)
            {
                Debug.Log(devices + " " + device);
            }
        }
    }
    
    [Serializable]
    public record DeviceMap 
    {
        public string deviceName;
        public List<KeyMap> keyMap;
    }
    
    [Serializable]
    public record KeyMap 
    {
        public KeyType keyType;
        public Sprite sprite;
    }
}