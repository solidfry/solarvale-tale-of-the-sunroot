using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem.InitialisationActions
{
    [CreateAssetMenu(fileName = "New Quest Initialisation", menuName = "Quest System/Quest Initialisation Action/SpawnObjectsAtPoint", order = 0)]
    public class SpawnObjectsAtPositions : InitialisationAction, IInitialisationAction
    {
        [SerializeField] List<ObjectEntry> objectsToSpawnAtPositions;
        
        List<GameObject> _spawnedObjects = new ();
        
        public override void Execute() => SpawnObjects();

        public override void Clear()
        {
            foreach (var obj in _spawnedObjects)
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
                    var spawned= Instantiate(obj.objectToSpawn, obj.objectToSpawn.transform.position,
                        obj.objectToSpawn.transform.rotation);
                    _spawnedObjects.Add(spawned);
                }
                else  
                {
                    var spawned= Instantiate(obj.objectToSpawn, obj.position, Quaternion.identity);
                    _spawnedObjects.Add(spawned);
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