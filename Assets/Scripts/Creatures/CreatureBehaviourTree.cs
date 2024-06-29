using System.Collections.Generic;
using Behaviour;
using UnityEngine;
using Behaviour.Tree.Nodes;
using UnityEngine.AI;
using Tree = Behaviour.Tree.Tree;

namespace Creatures
{
    [RequireComponent(typeof(Creature))]
    public class CreatureBehaviourTree : Tree
    {
        [Header("Creature Behaviour Tree")]
        [SerializeField] Creature creature;
        [SerializeField] Transform[] currentFoodSources = new Transform[5];
        [SerializeField] private LayerMask foodLayer;
        
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
            _agent = creature.GetAgent();
        }

        protected override Node SetupTree()
        {
            Node root = new Selector(new List<Node>
            {
                new Sequence(new List<Node>
                {
                    new CheckFoodInRange(this.creature, foodLayer),
                    new TaskGoToTarget(transform, _agent)
                }),
                new TaskPatrol(this.creature, _agent, currentFoodSources)
            });
            return root;
        }
        
        public void SetFoodSources(Transform[] foodSources)
        {
            currentFoodSources = foodSources;
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

        
   