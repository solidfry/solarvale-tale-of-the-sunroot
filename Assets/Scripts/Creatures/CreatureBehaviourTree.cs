using System.Collections.Generic;
using System.Linq;
using Behaviour.Pathfinding;
using Creatures.Stats;
using DG.Tweening;
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
        [SerializeField] int multiplierLimit = 10;
        [SerializeField] bool inDanger;
        
        Animator _animator;
        
        NavMeshAgent _agent;
        
        BehaviourTree _tree;
        CreatureStatsDataBase _stats;
        

        [SerializeField] private Transform enemy;
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int IsSearching = Animator.StringToHash("isSearching");
        private static readonly int IsEating = Animator.StringToHash("isEating");
        private static readonly int Speed = Animator.StringToHash("speed");

        public void Initialise()
        {
            if (creature == null)
            {
                creature = GetComponent<Creature>();
            }
            _agent = creature.GetAgent();
            _stats = creature.GetStats;
            _animator = creature.GetAnimator();

            Debug.Log(_animator);
            
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
            HandleMovementAnimations();
        }

        private void HandleMovementAnimations()
        {
            if (_animator == null) return;

            var speed = Mathf.Clamp01(_agent.velocity.magnitude / _stats.RunSpeedMultiplier);
            // Debug.Log(speed);            
            _animator.SetFloat(Speed, speed);
            
            switch (speed)
            {
                case 0:
                    _animator.SetBool(IsMoving, false);
                    break;
                default:
                    _animator.SetBool(IsMoving, true);
                    break;
            }
           
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
            
                Collider[] colliders = Physics.OverlapSphere(transform.position, sightRange, targetLayer);
                
                _animator.SetBool(IsSearching, true);
                
                foreach (var col in colliders)
                {
                    if (currentTargets.Contains(col.transform)) continue;
                    currentTargets.Add(col.transform);
                    sightRangeMultiplier = 1;
                    sightRange = _stats.SightRange;
                    // Debug.Log(creature + " found target " + col.transform);
                    return true;
                }
                
                if (currentTargets.Count == 0 || currentTargets[0] == null)
                {
                    IncrementSightRange();
                    
                    MultiplySightRange();
                    // Debug.Log(creature + " increased sight range to " + sightRange);
                    return false;
                }

                // Debug.Log(creature + " found target " + currentTargets[0]);
                return true;
            }));
            
            Leaf checkTargetValid = new Leaf("CheckTargetValid", new Condition(() => currentTargets.Count > 0));
            
            Leaf moveToTarget = new Leaf("MoveToTarget", new Condition(() => {
                if (currentTargets.Count > 0 && currentTargets[0] != null)
                {
                    RotateTowardsThenMove(currentTargets[0].position, _stats.TurningSpeed ,_stats.Speed);
                    // I want to turn the creature over time to face the target
                    return true;
                }
                return false;
            }));    
            
            Leaf isTargetInRangeForAction = new Leaf("IsTargetInRangeForAction", new Condition(() => {
                var isInRange = Vector3.Distance(_agent.transform.position, currentTargets[0].position) <= _agent.stoppingDistance + 1f && !_agent.pathPending; // + 1f to account for the stopping distance
                return isInRange;
            }));
            
            Leaf consume = new Leaf("ConsumeTarget", new DoActionWhileDelayingActionStrategy( _stats.FeedRate, () => {
                if (currentTargets.Count == 0) return;
                currentTargets[0].gameObject.SetActive(false);
                currentTargets.RemoveAt(0);
                // Debug.Log(creature + " finished eating");
                _animator.SetBool(IsEating, false);
                
                findAndMoveToTarget.Reset();
            }, () =>
            {
                // Debug.Log(creature + " is eating");
                _animator.SetBool(IsEating, true);
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

        private void RotateTowardsThenMove(Vector3 position, float rotationSpeed, float speed = 1f)
        {
            if (_agent == null) return;
            _agent.transform.DORotateQuaternion( 
                Quaternion.LookRotation(position - _agent.transform.position), 
                rotationSpeed).OnComplete(() =>
            {
                Move(position, speed);
            });
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
            return distance;
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

        
   