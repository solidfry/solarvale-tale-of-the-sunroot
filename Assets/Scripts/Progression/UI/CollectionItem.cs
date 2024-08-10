using Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Progression.UI
{
    public class CollectionItem : MonoBehaviour
    {
        [SerializeField] EntityData entityData;
        [SerializeField] private GameObject titleObject;
        [SerializeField] Image graphic;
        [SerializeField] TMP_Text titleText;
        [SerializeField] bool isDiscovered = false;

        public bool IsDiscovered
        {
            get => isDiscovered;
            private set
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
                if (entityData.Avatar != null)
                    SetGraphic(entityData.Avatar);
            }
        }
        
        public EntityData GetEntityData()
        {
            return entityData;
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
