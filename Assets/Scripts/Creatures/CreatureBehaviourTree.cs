using System.Collections.Generic;
using Behaviour;
using Behaviour.Tree;
using UnityEngine;
using Behaviour.Tree.Nodes;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Creatures
{
    [RequireComponent(typeof(Creature))]
    public class CreatureBehaviourTree : BehaviourTreeBase
    {
        [Header("Creature Behaviour Tree")]
        [SerializeField] Creature creature;
        [FormerlySerializedAs("currentFoodSources")] [SerializeField] Transform[] currentTargets = new Transform[5];
        [FormerlySerializedAs("foodLayer")] [SerializeField] private LayerMask targetLayer;
        
        NavMeshAgent _agent;

        private void OnValidate()
        {
            if (creature == null)
            {
                creature = GetComponent<Creature>();
            }
        }

        private void Awake()
        {
            creature = GetComponent<Creature>();
            _agent = creature.GetAgent();
        }

        protected override Node SetupTree()
        {
            var root = new Selector(new List<Node>
            {
                new Sequence(new List<Node>
                {
                    new CheckFoodInRange(creature, targetLayer),
                    new TaskGoToTarget(transform, _agent)
                }),
                new TaskPatrol(creature, _agent, currentTargets)
            });
            return root;
        }
        
        
        public int GetTargetSources(out Transform[] targetSources)
        {
            targetSources = currentTargets;
            return currentTargets.Length;
        }
        
        public int GetAllNonNullTargetSources(out Transform[] targetSources)
        {
            List<Transform> sources = new List<Transform>();
            foreach (var source in currentTargets)
            {
                if (source != null)
                {
                    sources.Add(source);
                }
            }
            targetSources = sources.ToArray();
            return sources.Count;
        }
        
        public void SetFoodSources(Transform[] targetSources)
        {
            currentTargets = targetSources;
        }

        private void OnDrawGizmos()
        {
            if (creature == null) return;
            if (creature.GetStats == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, creature.GetStats.SightRange);
        }
    }
    
}

        
   