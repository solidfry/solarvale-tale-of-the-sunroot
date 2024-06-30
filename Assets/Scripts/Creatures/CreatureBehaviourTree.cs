using System.Collections.Generic;
using System.Linq;
using Behaviour.Pathfinding;
using UnityEngine;
using UnityEngine.AI;
using Sequence = Behaviour.Pathfinding.Sequence;

namespace Creatures
{
    [RequireComponent(typeof(Creature))]
    public class CreatureBehaviourTree : MonoBehaviour
    {
        [Header("Creature Behaviour Tree")]
        [SerializeField] Creature creature;
        [SerializeField] List<Transform> currentTargets = new (5);
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] int sightRangeMultiplier = 1;
        [SerializeField] float sightRange = 5;

        NavMeshAgent _agent;
        
        BehaviourTree _tree;

        private void OnValidate()
        {
            if (creature == null)
            {
                creature = GetComponent<Creature>();
            }
        }

        private void Awake()
        {
            if (creature == null)
            {
                creature = GetComponent<Creature>();
            }
            _agent = creature.GetAgent();
            _tree = new BehaviourTree($"{creature.GetEntityData?.name}");
        }

        private void Start()
        {
            sightRange = creature.GetStats.SightRange * sightRangeMultiplier;
            
            Debug.Log(creature.CurrentSightRange);
            

            Leaf find = new Leaf("FindTarget", new Condition(() => {

                Collider[] colliders = Physics.OverlapSphere(transform.position, sightRange, targetLayer);

                foreach (var col in colliders)
                {
                    if (currentTargets.Contains(col.transform)) continue;
                    currentTargets.Add(col.transform);
                    sightRangeMultiplier = 1;
                    sightRange = creature.GetStats.SightRange * sightRangeMultiplier;
                    return true;
                }
                
                if (currentTargets.Count == 0)
                {
                    // I want to clamp these values to a max of 5
                    
                    sightRangeMultiplier = sightRangeMultiplier < 5 ? sightRangeMultiplier + 1 : 5;
                    
                    sightRange = creature.GetStats.SightRange * sightRangeMultiplier;
                    return false;
                }
                
                return true;
            }));

            Leaf moveToTarget = new Leaf("MoveToTarget", new Condition(() => {
                if (currentTargets.Count == 0) return false;
                _agent.SetDestination(currentTargets[0].position);
                return true;
            }));

            Leaf isTargetInRangeForAction = new Leaf("IsTargetInRangeForAction", new Condition(() => {
                var isInRange = Vector3.Distance(_agent.transform.position, currentTargets[0].position) <= _agent.stoppingDistance + 1f && !_agent.pathPending; // + 1f to account for the stopping distance
                return isInRange;
            }));

            Leaf consume = new Leaf("ConsumeTarget", new ActionStrategy(() => {
                currentTargets[0].gameObject.SetActive(false);
                currentTargets.RemoveAt(0);
                _tree.Reset();
            }));

            Sequence findAndMoveToTarget = new Sequence("FindAndMoveToTarget");
            findAndMoveToTarget.AddChild(find);
            findAndMoveToTarget.AddChild(moveToTarget);
            findAndMoveToTarget.AddChild(isTargetInRangeForAction);
            findAndMoveToTarget.AddChild(consume);

            _tree.AddChild(findAndMoveToTarget);
        }

        private void Update()
        {
            _tree.Process();
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
            Gizmos.color = currentTargets.Any(x => x is not null) ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }
    }
}

        
   