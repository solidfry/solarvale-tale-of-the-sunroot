using Behaviour;
using UnityEngine;
using Behaviour.Tree.Nodes;
using UnityEngine.AI;
using Tree = Behaviour.Tree.Tree;

namespace Creatures
{
    [RequireComponent(typeof(Creature))]
    public class CreatureBehaviourTreeBase : Tree
    {
        
        [Header("Creature Behaviour Tree")]
        [SerializeField] Creature creature;
        [SerializeField] Transform[] foodSources;
        NavMeshAgent _agent;

        private void OnValidate()
        {
            if (_agent == null)
            {
                _agent = creature.Agent;
            }
        }

        protected override Node SetupTree()
        {
            Node root = new TaskPatrol(_agent, transform, foodSources);
            return root;
        }

      
    }
}

        
   