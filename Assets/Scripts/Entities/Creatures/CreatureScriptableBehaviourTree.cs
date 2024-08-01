using System.Linq;
using Behaviour.ScriptableBehaviour;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace Entities.Creatures
{
    [RequireComponent( typeof(Creature))]
    public class CreatureScriptableBehaviourTree : MonoBehaviour
    {
        [SerializeField] private bool pauseBehaviours;
        [SerializeField] BehaviourTreeSo behaviorTreeSo;
        [SerializeField] private Transform target;
        
        public bool IsBehavioursPaused => pauseBehaviours;
        
        private NavMeshAgent _agent;
        private Creature _creature;
        private List<IEdible> _currentTargets = new ();
        private Transform _enemy;
        private BehaviourTreeContext _context;

        public LayerMask targetLayer;

        void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _creature = GetComponent<Creature>();

            _context = new BehaviourTreeContext(this, _agent, target, _creature, _currentTargets, _enemy, targetLayer);
        }
        
        public void Run()
        {
            if (IsBehavioursPaused) return;
            if (behaviorTreeSo != null)
            {
                behaviorTreeSo.Process(_context);
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        // Method to update the current targets in the context
        public void UpdateCurrentTargets(List<IEdible> newTargets)
        {
            _currentTargets = newTargets;
        }
        
        private void OnDrawGizmos()
        {
            if (_creature == null) return;
            if (_creature.GetStats == null) return;
            Gizmos.color = _context.CurrentTargets.Any(x => x is not null) ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, _creature.CurrentSightRange);
        }

        public void SetPauseBehaviours(bool value)
        {
            pauseBehaviours = value;
        }
    }

}