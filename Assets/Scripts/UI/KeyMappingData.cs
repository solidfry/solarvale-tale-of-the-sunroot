using System.Collections.Generic;
using UnityEngine;
using UI;
namespace UI
{
    [CreateAssetMenu(fileName = "KeyMapping", menuName = "UI/KeyMappingData", order = 0)]
    public class KeyMappingData : ScriptableObject
    {
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
    }
    
    [System.Serializable]
    public record KeyMap ()
    {
        public KeyType keyType;
        public Sprite sprite;
    }
}