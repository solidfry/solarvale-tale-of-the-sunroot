using System.Collections.Generic;
using System.Linq;
using Core;
using Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Progression
{
    public class CollectionUIController : MonoBehaviour
    {
        [SerializeField] private CollectionManager collectionManager;
        [Header("UI Elements")]
        [SerializeField] private CollectionItem collectionItemPrefab;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;

        private Dictionary<EntityData, CollectionItem> _entitiesCollectionItems = new();
        private bool _isFirstLoad = true;

        private void Awake()
        {
            gridLayoutGroup ??= GetComponentInChildren<GridLayoutGroup>();
            collectionManager = FindObjectOfType<GameManager>()?.CollectionManager;

            if (collectionManager != null)
            {
                SubscribeToCollectionManagerEvents();
            }
        }

        private void OnEnable()
        {
            if (_isFirstLoad)
            {
                Debug.Log($"Initial collection manager discoveries: {collectionManager?.GetDiscoveries().Count}");
                LoadAll(collectionManager?.GetDiscoveries());
                UpdateOverlap();
                SortElementsByDiscovered();
                _isFirstLoad = false;
            }
            else if (collectionManager?.HasDictionaryChanged == true && !_isFirstLoad)
            {
                Debug.Log("Dictionary has changed and was updated from OnEnable");
                UpdateOverlap();
                SortElementsByDiscovered();
            }
        }
        
        private void SortElementsByDiscovered()
        {
            var discoveredItems = _entitiesCollectionItems.Values.Where(item => item.IsDiscovered).OrderByDescending(item => item.GetEntityData().Name).ToList();
            var undiscoveredItems = _entitiesCollectionItems.Values.Where(item => !item.IsDiscovered).ToList();

            foreach (var item in discoveredItems)
            {
                item.transform.SetAsFirstSibling();
            }

            foreach (var item in undiscoveredItems)
            {
                item.transform.SetAsLastSibling();
            }
        }

        private void SubscribeToCollectionManagerEvents()
        {
            collectionManager.OnEntityListLoaded += LoadAll;
            collectionManager.OnEntityDiscovered += UpdateEntityUI;
        }

        private void UpdateEntityUI(EntityData entity)
        {
            if (!_entitiesCollectionItems.TryGetValue(entity, out var collectionItem) || collectionItem.IsDiscovered) return;
            collectionItem.SetDiscovered(true);
            Debug.Log($"Updated entity UI for {entity.Name} to discovered");
        }

        private void LoadAll(Dictionary<EntityData, bool> entities)
        {
            Debug.Log("Loading all entities");
            if (entities == null) return;

            _entitiesCollectionItems = entities.Keys.ToDictionary(
                entity => entity,
                entity =>
                {
                    var collectionItem = Instantiate(collectionItemPrefab, gridLayoutGroup.transform);
                    collectionItem.SetEntityData(entity);
                    collectionItem.SetDiscovered(entities[entity]);
                    Debug.Log($"Added entity to collection: {entity.Name} and is discovered: {entities[entity]}");
                    return collectionItem;
                });
        }

        private void UpdateOverlap()
        {
            var dict = collectionManager?.GetDiscoveries();
            if (dict == null) return;

            var entitiesToUpdate = dict.Where(kv => kv.Value)
                                       .Select(kv => kv.Key)
                                       .ToList();

            entitiesToUpdate.ForEach(UpdateEntityUI);
            collectionManager.DictionaryUpdated();
            Debug.Log("Updated overlap");
        }
    }
}
