using Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Progression
{
    public class CollectionItem : MonoBehaviour
    {
        [SerializeField] EntityData entityData;
        [SerializeField] private GameObject titleObject;
        [SerializeField] Image graphic;
        [SerializeField] TMP_Text titleText;
        [SerializeField] bool isDiscovered = false;

        bool IsDiscovered
        {
            get => isDiscovered;
            set
            {
                isDiscovered = value;
                titleObject.SetActive(value);
                graphic.gameObject.SetActive(value);
            }
        }

        private void Start()
        {
            if (entityData != null)
            {
                // SetGraphic(entityData.Graphic);
                // SetDiscovered(entityData.IsDiscovered);
                SetName(entityData.Name);
            }
        
            SetDiscovered(false);
        }

    
        public void SetGraphic(Sprite sprite)
        {
            graphic.sprite = sprite;
        }
    
        public void SetDiscovered(bool discovered)
        {
            IsDiscovered = discovered;
        }
    
        public void SetEntityData(EntityData data)
        {
            entityData = data;
        }
    
        [ContextMenu("Set Discovered True")]
        public void SetDiscoveredTrue()
        {
            IsDiscovered = true;
        }
    
        [ContextMenu("Set Discovered False")]
        public void SetDiscoveredFalse()
        {
            IsDiscovered = false;
        }
    
        public string EntityName => entityData.Name;
    
        void SetName(string name)
        {
            if (titleText is null)
            {
                titleText = titleObject.GetComponentInChildren<TMP_Text>();
            }
        
            titleText.text = name;
        }
    }
}
