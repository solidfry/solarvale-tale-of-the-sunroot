#if GENA_PRO
using GeNa.Core;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gaia
{
    /// <summary>
    /// Simple Spawn Extension for demo / debug purposes. Just writes some info to the console when being executed.
    /// </summary>
    public class GeNaProSpawnExtension : MonoBehaviour, ISpawnExtension
    {
        public string Name { get { return "GeNaProSpawnExtension"; } }

        public bool AffectsHeights => false;

        public bool AffectsTextures => false;

        public GameObject m_genaSpawnerPrefab;
#if GENA_PRO
        private GeNa.Core.GeNaSpawner m_genaSpawnerInstance;
#endif

        public void Close()
        {
            ////Debug.Log("Spawn Extension is closing down.");
            //if (m_genaSpawnerInstance != null)
            //{
            //    DestroyImmediate(m_genaSpawnerInstance.gameObject);
            //}
        }

        public void Init(Spawner spawner)
        {
#if GENA_PRO
            //Debug.Log("Spawn Extension starting up.");
            if (m_genaSpawnerPrefab == null)
            {
                Debug.LogWarning("GeNa Spawn Extension '" + Name + "' does not have a GeNa Spawner Prefab assigned.");
                return;
            }
            m_genaSpawnerInstance = m_genaSpawnerPrefab.GetComponent<GeNa.Core.GeNaSpawner>();

            if (m_genaSpawnerInstance == null)
            {
                Debug.LogWarning("Could not find a GeNa Spawner component on the prefab for GeNa Spawn Extension '" + Name + "'. Does this prefab use a GeNa Spawner component on the top level?");
            }
#endif

        }

        public void Spawn(Spawner spawner, Transform target, int ruleIndex, int instanceIndex, SpawnExtensionInfo spawnExtensionInfo)
        {
#if GENA_PRO
            //Debug.Log("Spawn Extension spawning.");
            if (m_genaSpawnerInstance != null)
            {
                m_genaSpawnerInstance.Load();
                GeNaSpawnerData data = m_genaSpawnerInstance.SpawnerData;
                SpawnCall spawnCall = GeNaSpawnerInternal.GenerateSpawnCall(data, spawnExtensionInfo.m_position);
                m_genaSpawnerInstance.Save();
                GeNaSpawnerInternal.SetSpawnOrigin(data, spawnCall, true);
                m_genaSpawnerInstance.Spawn(spawnExtensionInfo.m_position);
                m_genaSpawnerInstance.GetParent().SetParent(target);
            }
#endif
        }

        public void Delete(Transform target)
        {
            DestroyImmediate(target.gameObject);
        }
    }
}
