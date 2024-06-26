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
        [SerializeField] List<DeviceMap> deviceMaps = new();
        [SerializeField] List<KeyMap> keyMap = new();
        
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
        
#if  UNITY_EDITOR
        [ContextMenu("PopulateDeviceMaps")]
        public void PopulateDeviceMaps()
        {
            ClearDeviceMaps();

            var schemes = inputActionAsset.controlSchemes;
            var maps = inputActionAsset.actionMaps;

            foreach (var scheme in schemes)
            {
                var deviceMap = new DeviceMap
                {
                    deviceName = scheme.name,
                    keyTypeMap = new List<KeyTypeMap>()
                };

                foreach (var map in maps)
                {
                    foreach (var binding in map.bindings)
                    {
                        if (binding.groups.Contains(scheme.bindingGroup))
                        {
                            var keyTypeMap = new KeyTypeMap
                            {
                                name = binding.action,
                                keyType = MapActionPathToKeyType(binding.path),
                                path = binding.path
                            };
                            deviceMap.keyTypeMap.Add(keyTypeMap);
                        }
                    }
                }

                deviceMaps.Add(deviceMap);
            }
        }

        [ContextMenu("ClearDeviceMaps")]
        public void ClearDeviceMaps()
        {
            deviceMaps.Clear();
        }

        
        private KeyType MapActionPathToKeyType(string path)
        {
            var subPath = path.Substring(path.IndexOf("/") + 1).Replace("/", string.Empty);
            
            var keyTypes = Enum.GetNames(typeof(KeyType));
            
            foreach (var keyType in keyTypes)
            {
                if (path.Contains("keyboard", StringComparison.OrdinalIgnoreCase))
                {
                    return KeyType.Keyboard;
                }
                if (subPath.Length == 1) // if the path is just a single character, it's a button
                {
                    return KeyType.Button;
                }
                
                
                
                if (subPath.Contains(keyType, StringComparison.OrdinalIgnoreCase))
                {
                    return (KeyType) Enum.Parse(typeof(KeyType), keyType);
                }
            }
            
            return KeyType.None;
        }
#endif
        
        public KeyTypeMap GetSpriteByDeviceAndAction(string deviceName, string actionName)
        {
            return deviceMaps.Find( x => x.deviceName == deviceName)?.keyTypeMap.Find( x => x.name == actionName);
        }
    }

    [Serializable]
    public class DeviceMap 
    {
        public string deviceName;
        public List<KeyTypeMap> keyTypeMap;
    }

    [Serializable] 
    public class KeyTypeMap
    {
        public new string name;
        public KeyType keyType;
        public string path;
    }

    [Serializable]
    public class KeyMap 
    {
        public KeyType keyType;
        public Sprite sprite;
    }
}
