using System.Collections.Generic;
using System.Linq;
using Behaviour.Pathfinding;
using DG.Tweening;
using Entities.Creatures.Stats;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Sequence = Behaviour.Pathfinding.Sequence;

namespace Entities.Creatures
{
    [RequireComponent(typeof(Creature))]
    public class CreatureBehaviourTree : MonoBehaviour
    {
        [Header("Creature Behaviour Tree")]
        [SerializeField] Creature creature;

        [SerializeField] private LayerMask targetLayer;
        [SerializeField] int sightRangeMultiplier = 1;
        [SerializeField] float sightRange = 5;
        [SerializeField] int multiplierLimit = 10;
        [SerializeField] bool inDanger;

        [SerializeField] private int currentTargetCount;
        
        #region Members
        List<IEdible> _currentTargets = new (5);
        NavMeshAgent _agent;
        BehaviourTree _tree;
        CreatureStatsData _stats;
        Tween _rotationAndMoveTween;
        #endregion
        
        [Space(10)]
        [Header("Events")]
        [SerializeField] UnityEvent onFindTarget;
        [SerializeField] UnityEvent onTargetFound;
        [SerializeField] UnityEvent onConsumingEnter;
        [SerializeField] UnityEvent onConsumingEnd;
        [SerializeField] UnityEvent onDangerEnter;
        [SerializeField] UnityEvent onDangerEnd;
        
        [SerializeField] private Transform enemy;
        
        public void Initialise()
        {
            if (creature == null)
            {
                creature = GetComponent<Creature>();
            }
            _agent = creature.GetAgent();
            _stats = creature.GetStats;
            
            sightRange = _stats.SightRange * sightRangeMultiplier;

            _tree = new BehaviourTree($"{creature.GetEntityData?.name}");
            CreateBehaviourTree();
        }
        

        private void OnValidate()
        {
            if (creature == null)
            {
                creature = GetComponent<Creature>();
            }
            if (creature.GetStats != null)
                _stats = creature.GetStats;
        }
        
        private void MultiplySightRange()
        {
            sightRange = creature.GetStats.SightRange * sightRangeMultiplier;
        }

        private void IncrementSightRange()
        {
            sightRangeMultiplier = sightRangeMultiplier < multiplierLimit ? sightRangeMultiplier + 1 : multiplierLimit;
        }

        private void Update()
        {
            _tree.Process();
        }

        private void CreateBehaviourTree()
        {
            PrioritySelector actions = new PrioritySelector("Creature Logic");
            
            Sequence runToSafety = new Sequence("Run to safety", 100);

            bool IsSafe()
            {
                if (!inDanger)
                {
                    runToSafety.Reset();
                    return false;
                }
                
                return true;
            }
            
            runToSafety.AddChild(new Leaf("IsSafe?", new Condition(IsSafe)));
            
            runToSafety.AddChild( new Leaf("Run away from target", 
                new MoveAwayFromTarget( 
                    _agent, enemy,
                    _stats.RunSpeedMultiplier, _stats.DangerRunTime)));
            
            runToSafety.AddChild( new Leaf("IsSafe?", new Condition(CheckInDanger)));
  
            
            Sequence findAndMoveToTarget = new Sequence("FindAndMoveToTarget", 50);

            Leaf find = new Leaf("FindTarget", new Condition(() => {
                
                if (_currentTargets.Count > 0)
                {
                    onTargetFound?.Invoke();
                    return true;
                }
                
                Collider[] colliders = Physics.OverlapSphere(transform.position, sightRange, targetLayer);
                onFindTarget?.Invoke();
                
                if (colliders.Length == 0)
                {
                    IncrementSightRange();
                    MultiplySightRange();
                    Debug.Log("No colliders found");
                    return false;
                }
                
                foreach (var col in colliders)
                {
                    if (col.TryGetComponent(out IEdible edible))
                    {
                        if (edible.IsConsumed || edible.IsOccupied)
                        {
                            RemoveTarget(edible);
                            return false;
                        }
                        AddTarget(edible);
                        sightRangeMultiplier = 1;
                        sightRange = _stats.SightRange;
                        onTargetFound?.Invoke();
                        Debug.Log("Edible found");
                        return true;
                    }
                }

                // Debug.Log("No edible found");
                
                return true;
            }));
            
            Leaf checkTargetValid = new Leaf("CheckTargetValid", new Condition(() => _currentTargets.Count > 0));
            
            Leaf moveToTarget = new Leaf("MoveToTarget", new Condition(() => {
                if (_currentTargets.Count > 0 && _currentTargets[0] != null)
                {
                    Move(_currentTargets[0].GetTransform.position, _stats.Speed);
                    return true;
                }
                return false;
            }));    
            
            Leaf isTargetInRangeForAction = new Leaf("IsTargetInRangeForAction", new Condition(() => {
                // Ensure the path is complete
                if (_agent.pathStatus != NavMeshPathStatus.PathComplete) return false;

                // Check if the remaining distance is less than or equal to the stopping distance
                var isInRange = _agent.remainingDistance <= _agent.stoppingDistance + _stats.Width;
                // _agent.destination = _agent.transform.position;
                _agent.isStopped = isInRange;
                return isInRange;
            }));
            
            Leaf consume = new Leaf("ConsumeTarget", new DoActionWhileDelayingActionStrategy( _stats.FeedRate, () => {
                if (_currentTargets.Count == 0) return;
                // This needs to be a coroutine
                _currentTargets[0].Consume();
                RemoveTargetAtPosition();
                onConsumingEnd?.Invoke();
                findAndMoveToTarget.Reset();
            }, () =>
            {
                onConsumingEnter?.Invoke();
            }));
            
            findAndMoveToTarget.AddChild(find);
            findAndMoveToTarget.AddChild(checkTargetValid);
            findAndMoveToTarget.AddChild(moveToTarget);
            findAndMoveToTarget.AddChild(isTargetInRangeForAction);
            findAndMoveToTarget.AddChild(consume);
            
            actions.AddChild(runToSafety);
            actions.AddChild(findAndMoveToTarget);
            
            _tree.AddChild(actions);
        }
        
        void AddTarget(IEdible targetToAdd)
        {
            currentTargetCount++;
            _currentTargets.Add(targetToAdd);
        }

        void RemoveTargetAtPosition(int position = 0)
        {
            _currentTargets.RemoveAt(position);
            currentTargetCount--;
        }

        void RemoveTarget(IEdible targetToRemove)
        {
            _currentTargets.Remove(targetToRemove);
            currentTargetCount--;
        }

        private void Move(Vector3 position, float speed = 1f)
        {
            // _animator.SetBool(IsWalking, true);
            _agent.SetDestination(position);
            _agent.speed = speed;
        }
        
        private void Run(Vector3 position, float speed = 1f)
        {
            Move(position, speed * _stats.RunSpeedMultiplier);
        }

        public void SetEnemy(Transform enemy)
        {
            this.enemy = enemy;
        }
        
        public void SetInDanger(bool danger)
        {
            inDanger = danger;
        }
        
        public bool CheckInDanger()
        {
            if (enemy is null) return false;
            var distance =  Vector3.Distance(transform.position, enemy.position) <= _stats.DangerDetectionRange;
            // UEvent when in danger
            if (distance)
            {
                // Debug.Log("In danger");
                onDangerEnter?.Invoke();
            }
            else
            {
                // Debug.Log("Not in danger");
                onDangerEnd?.Invoke();
            }
            
            return distance;
        }

        private void OnDrawGizmos()
        {
            if (creature == null) return;
            if (creature.GetStats == null) return;
            Gizmos.color = _currentTargets.Any(x => x is not null) ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

      
    }
}

        
   