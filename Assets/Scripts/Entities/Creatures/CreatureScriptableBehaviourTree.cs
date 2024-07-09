using System.Linq;
using Behaviour.ScriptableBehaviour;

namespace Entities.Creatures
{
    using UnityEngine;
    using UnityEngine.AI;
    using System.Collections.Generic;

    public class CreatureScriptableBehaviourTree : MonoBehaviour
    {
        [SerializeField] BehaviourTreeSo behaviorTreeSO;
        private NavMeshAgent agent;
        private Creature creature;
        private List<IEdible> currentTargets = new ();
        [SerializeField] private Transform target;
        [SerializeField] private BehaviourTreeContext context;

        private Transform enemy;

        public LayerMask targetLayer;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            creature = GetComponent<Creature>();

            context = new BehaviourTreeContext(this, agent, target, creature, currentTargets, enemy, targetLayer);
        }

        void Update()
        {
            if (behaviorTreeSO != null)
            {
                behaviorTreeSO.Process(context);
                // Debug.Log("Processing behaviour tree");
            }
        }
        

        // Method to update the target in the context
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        // Method to update the current targets in the context
        public void UpdateCurrentTargets(List<IEdible> newTargets)
        {
            currentTargets = newTargets;
        }
        
        private void OnDrawGizmos()
        {
            if (creature == null) return;
            if (creature.GetStats == null) return;
            Gizmos.color = context.CurrentTargets.Any(x => x is not null) ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, creature.CurrentSightRange);
        }
    }

}