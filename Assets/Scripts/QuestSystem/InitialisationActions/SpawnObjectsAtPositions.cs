using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem.InitialisationActions
{
    [CreateAssetMenu(fileName = "New Quest Initialisation", menuName = "Quest System/Quest Initialisation Action/SpawnObjectsAtPoint", order = 0)]
    public class SpawnObjectsAtPositions : InitialisationAction, IInitialisationAction
    {
        [SerializeField] List<ObjectEntry> objectsToSpawnAtPositions;
        
        [SerializeField] List<GameObject> spawnedObjects;
        
        public override void Execute()
        {
            SpawnObjects();
        }
        
        public override void Clear()
        {
            foreach (var obj in spawnedObjects)
            {
                Destroy(obj);
            }
        }

        private void SpawnObjects()
        {
            foreach (var obj in objectsToSpawnAtPositions)
            {
                if (obj.useObjectTransform)
                {
                    Instantiate(obj.objectToSpawn, obj.objectToSpawn.transform.position,
                        obj.objectToSpawn.transform.rotation);
                    spawnedObjects.Add(obj.objectToSpawn);
                }
                else  
                {
                    Instantiate(obj.objectToSpawn, obj.position, Quaternion.identity);
                    spawnedObjects.Add(obj.objectToSpawn);
                }
            }
        }
    }
    
    [Serializable]
    public record ObjectEntry
    {
        public Vector3 position;
        public GameObject objectToSpawn;
        public bool useObjectTransform;
    }
}