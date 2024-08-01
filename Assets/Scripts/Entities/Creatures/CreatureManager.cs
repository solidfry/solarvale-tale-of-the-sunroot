using System.Collections.Generic;
using Events;
using Player;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Entities.Creatures
{
    public class CreatureManager : MonoBehaviour
    {
        [SerializeField] float cullingDistance = 100f;

        [SerializeField] private bool useCulling;
        [SerializeField] private bool pauseCreatureBehaviour;
        [SerializeField] private Transform player;
        [SerializeField] List<Creature> creatures = new ();
        
        private NativeArray<Vector3> creaturePositions;
        private NativeArray<Vector3> playerPosition;
        private NativeArray<bool> creatureStates;
        
        private void Awake()
        {
            GlobalEvents.OnRegisterCreatureEvent += AddCreature;
            GetPlayer();
        }
        
        private void GetPlayer()
        {
            if (player == null)
            {
                player = FindObjectOfType<PlayerManager>().GetPlayerTransform();
            }
        }
        
        public static float DistanceToPlayer(Transform transform, Transform player)
        {
            return Vector3.Distance(transform.position, player.position);
        }
        
        public static bool IsWithinCullingDistance(Transform transform, Transform player, float cullingDistance)
        {
            return DistanceToPlayer(transform, player) < cullingDistance;
        }
        
        private void Update()
        {
            if (pauseCreatureBehaviour) return;
            Run();
        }

        private void Run()
        {
            if (creatures.Count == 0) return;

            if (useCulling)
            {
                InitJobsData();
                
                var calculateDistanceToPlayerJob = new CalculateDistanceToPlayer
                {
                    creaturePositions = creaturePositions,
                    playerPosition = playerPosition,
                    cullingDistance = cullingDistance,
                    creatureStates = creatureStates
                };
                
                JobHandle jobHandle = calculateDistanceToPlayerJob.Schedule(creatures.Count, 64);
                jobHandle.Complete();

                ApplyJobResults();
            } else {
                foreach (var creature in creatures)
                {
                    if (creature.GetBehaviourTree.IsBehavioursPaused) continue;
                    creature.GetBehaviourTree?.Run();
                }
            }
            
        }

        private void InitJobsData()
        {
            if (!creaturePositions.IsCreated || creaturePositions.Length != creatures.Count)
                creaturePositions = new NativeArray<Vector3>(creatures.Count, Allocator.Persistent);
            if (!creatureStates.IsCreated || creatureStates.Length != creatures.Count)
                creatureStates = new NativeArray<bool>(creatures.Count, Allocator.Persistent);
            if (!playerPosition.IsCreated)
                playerPosition = new NativeArray<Vector3>(1, Allocator.Persistent);
            
            playerPosition[0] = player.position;
            
            for (int i = 0; i < creatures.Count; i++)
            {
                creaturePositions[i] = creatures[i].transform.position;
            }

        }

        private void ApplyJobResults()
        {
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatureStates[i])
                {
                    creatures[i].gameObject.SetActive(true);
                    if (creatures[i].GetBehaviourTree.IsBehavioursPaused) continue;
                    creatures[i].GetBehaviourTree?.Run();
                }
                else
                {
                    creatures[i].gameObject.SetActive(false);
                }
            }
        }

        public void AddCreature(Creature creature)
        {
            if (creatures.Contains(creature)) return;
            creatures.Add(creature);
        }
        
        private void OnDestroy()
        {
            GlobalEvents.OnRegisterCreatureEvent -= AddCreature;
            creatures.Clear();
            if (creaturePositions.IsCreated)
                creaturePositions.Dispose();
            if (playerPosition.IsCreated)
                playerPosition.Dispose();
            if (creatureStates.IsCreated)
                creatureStates.Dispose();
        }
        
        private void OnDrawGizmos()
        {
            if (!useCulling) return;
            if (player is null) return;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(player.position, cullingDistance);
        }
    }
}