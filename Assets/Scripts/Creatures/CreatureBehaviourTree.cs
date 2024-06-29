using System.Collections.Generic;
using Behaviour;
using Behaviour.Tree;
using UnityEngine;
using Behaviour.Tree.Nodes;
using Creatures.Behaviours;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Creatures
{
    [RequireComponent(typeof(Creature))]
    public class CreatureBehaviourTree : BehaviourTreeBase
    {
        [Header("Creature Behaviour Tree")]
        [SerializeField] Creature creature;
        [SerializeField] List<Transform> currentTargets = new (5);
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
            Node root = new Selector(new List<Node>
            {
                new Sequence( new List<Node>
                {
                    new TaskCheckTargetInRangeForAction(creature, targetLayer),
                    new TaskAction(creature, () =>
                    {
                        Debug.Log("Eating target");
                         var target = currentTargets[0];
                        currentTargets.RemoveAt(0);
                        Destroy(target.gameObject);
                    }) 
                }),
                new Sequence(new List<Node>
                {
                    new TaskFindTargetInSightRange(creature, targetLayer, ref currentTargets),
                    new TaskGoToTarget(transform, _agent)
                }),
                new TaskPatrol(creature, _agent, ref currentTargets)
            });
            return root;
        }
        
        public ref List<Transform> GetFoodSourcesAsRef()
        {
            return ref currentTargets;
        }
        
        public void SetFoodSources(List<Transform> targetSources)
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

        
   