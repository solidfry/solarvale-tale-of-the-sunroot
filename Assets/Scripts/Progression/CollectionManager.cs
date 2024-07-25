using System.Collections.Generic;
using Entities;
using Events;
using Photography;
using UnityEngine;

namespace Progression
{
    public class CollectionManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] EntityList entityList;

        private readonly Dictionary<EntityData, bool> _entityDiscovered = new();

        private void Awake() => CreateEntityDictionary();

        void CreateEntityDictionary()
        {
            foreach (var entity in entityList.Entities)
            {
                _entityDiscovered.Add(entity, false);
            }
        }

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
                _entityDiscovered[entityList.Entities.Find(entity => entity.Name == entityString)] = true;
            }
        }
        
        public Dictionary<EntityData, bool> GetDiscoveries()
        {
            return _entityDiscovered;
        }
        

    }
}
