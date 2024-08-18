using System;
using System.Collections.Generic;
using Core;
using Entities;
using Events;
using Photography;
using UnityEngine;

namespace Progression
{
    public class CollectionManager : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;
        [Header("Data")]
        [SerializeField] EntityList entityList;
        private readonly Dictionary<EntityData, bool> _entitiesDiscovered = new();
        
        [field: SerializeField] public bool HasDictionaryChanged { get; private set; } 
        
        public event Action<Dictionary<EntityData, bool>> OnEntityListLoaded;
        public event Action<EntityData> OnEntityDiscovered;

        private void Awake()
        {
            if (gameManager is null)
                gameManager = GetComponentInParent<GameManager>();
            
        }

        private void Start()
        {
            CreateEntityDictionary();
            gameManager.PhotoManager.OnPhotoAdded += UpdateDiscovery;
        }
        
        private void OnDisable()
        {
            gameManager.PhotoManager.OnPhotoAdded -= UpdateDiscovery;
        }
        
        void CreateEntityDictionary()
        {
            if (entityList is null || entityList.Entities is null)
            {
                Debug.LogError("Entity list or its entities cannot be null.");
                return;
            }
            
            List<EntityData> distinctEntities = new List<EntityData>();
            
            foreach (var photo in gameManager.PhotoManager.GetPhotos())
            {
                if (photo.EntitiesInPhoto == null) return;
                foreach (string entityString in photo.EntitiesInPhoto)
                {
                    var entity = entityList.Entities.Find(entity => entity.Name == entityString);
                    if (!distinctEntities.Contains(entity))
                    {
                        distinctEntities.Add(entity);
                    }
                }
            }
            
            InitialiseDiscoveries(distinctEntities);
        }

        private void InitialiseDiscoveries(List<EntityData> distinctEntities)
        {
            foreach (var entity in entityList.Entities)
            {
                _entitiesDiscovered.Add(entity, distinctEntities.Contains(entity));
                // Debug.Log("Entity added to dictionary: " + entity.Name);
            }
            HasDictionaryChanged = true;
            OnEntityListLoaded?.Invoke(_entitiesDiscovered);
        }
        
        private void UpdateDiscovery(PhotoData photo)
        {
            if (photo.EntitiesInPhoto == null) return;
            // Debug.Log("Checking discovery" + photo.EntitiesInPhoto);
            foreach (string entityString in photo.EntitiesInPhoto)
            {
                var entity = entityList.Entities.Find(entity => entity.Name == entityString);
                if (entity is null) return;
                if (_entitiesDiscovered.ContainsKey(entity) && !_entitiesDiscovered[entity])
                {
                    _entitiesDiscovered[entity] = true;
                    OnEntityDiscovered?.Invoke(entity);
                    GlobalEvents.OnNewEntityDiscovered?.Invoke(entity);
                    // Debug.Log("Entity discovered: " + entity.Name);
                }
                // Debug.Log("Entity discovered: " + entity.Name);
            }
            HasDictionaryChanged = true;
        }
        
        public Dictionary<EntityData, bool> GetDiscoveries()
        {
            return _entitiesDiscovered;
        }

        public EntityList GetEntityList()
        {
            return entityList;
        }
        
        public void DictionaryUpdated()
        {
            HasDictionaryChanged = false;
        }
    }
}
