using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Creatures
{
    public class CreatureManager : MonoBehaviour
    {
        [SerializeField] float cullingDistance = 100f;

        [SerializeField] private bool useCulling;
        [SerializeField] private bool pauseCreatureBehaviour;
        [SerializeField] private Transform player;
        [SerializeField] List<Creature> creatures = new ();
        
        
        
        private void Awake()
        {
            if (player == null)
                player = GameObject.FindWithTag("Player").transform;
            GlobalEvents.OnRegisterCreatureEvent += AddCreature;
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
            creatures.ForEach(creature =>
            {
                ToggleCreature(creature);
                creature.GetBehaviourTree?.Run();
            });
        }

        private void ToggleCreature(Creature creature)
        {
            if (!useCulling) return;
            if (!IsWithinCullingDistance(creature.transform, player, cullingDistance))
            {
                creature.enabled = false;
                return;
            }
            else
            {
                creature.enabled = true;
            }
        }

        public void AddCreature(Creature creature)
        {
            if (creatures.Contains(creature)) return;
            creatures.Add(creature);
        }

        public void RemoveCreature(Creature creature) => creatures.Remove(creature);

        public void RemoveAllCreatures()
        {
            creatures.Clear();
        }

        public List<Creature> GetCreatures()
        {
            return creatures;
        }
        
        private void OnDestroy()
        {
            GlobalEvents.OnRegisterCreatureEvent -= AddCreature;
            creatures.Clear();
        }
        
        private void OnDrawGizmos()
        {
            if (!useCulling) return;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(player.position, cullingDistance);
        }
    }
}