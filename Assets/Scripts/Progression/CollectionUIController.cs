using System.Collections.Generic;
using Entities;
using Events;
using Photography;
using UnityEngine;
using UnityEngine.UI;

namespace Progression
{
    public class CollectionUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] CollectionItem collectionItemPrefab;
        [SerializeField] GridLayoutGroup collectionItemParent;
    
        [Header("Data")]
        [SerializeField] EntityList entityList;
    
        [SerializeField] List<CollectionItem> collectionItems = new ();
    
        private void Start() => LoadAll();

        private void OnEnable()
        {
            GlobalEvents.OnPhotoKeptEvent += CheckDiscovery;
        }
    
        private void OnDisable()
        {
            GlobalEvents.OnPhotoKeptEvent -= CheckDiscovery;
        }

        private void CheckDiscovery(PhotoData photo)
        {
            if (photo.EntitiesInPhoto == null) return;
            Debug.Log("Checking discovery" + photo.EntitiesInPhoto);
            foreach (string entityString in photo.EntitiesInPhoto)
            {
                if (EntityExists(entityString))
                {
                    collectionItems.Find(item => item.EntityName == entityString).SetDiscovered(true);
                    Debug.Log("Entity exists: " + entityString);
                }
            }
        }
    
        private bool EntityExists(string entityString) => entityList.Entities.Exists(entity => entity.Name == entityString);
    
        private void LoadAll()
        {
            if (entityList is null) return;
            entityList.Entities.ForEach(AddEntityToCollection);
        }

        private void AddEntityToCollection(EntityData entity)
        {
            CollectionItem collectionItem = Instantiate(collectionItemPrefab, collectionItemParent.transform);
            collectionItem.SetEntityData(entity);
        }
    }
}
