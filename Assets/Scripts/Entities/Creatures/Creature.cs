using System.Collections;
using Behaviour.ScriptableBehaviour;
using DG.Tweening;
using Entities.Creatures.Movement;
using Entities.Creatures.Stats;
using Events;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Entities.Creatures
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Creature : MonoBehaviour, IEntity<CreatureEntityData>
    {
        [SerializeField] CreatureEntityData entityData;
        [SerializeField] Rigidbody rigidBody;
        [SerializeField] NavMeshAgent agent;
        [SerializeField] CapsuleCollider capsule;
        [SerializeField] Animator animator;
        [SerializeField] public GameObject model;
        
        [field: SerializeField] public float CurrentSightRange { get; set; } = 5;
        [field: SerializeField] public float CurrentSightRangeMultiplier { get; set; } = 1;
        [field: SerializeField] public int CurrentMultiplierLimit { get; private set; } = 20;
        
        // [SerializeField] CreatureBehaviourTree behaviourTree;
        
        CreatureStatsData _stats;
        // public CreatureBehaviourTree GetBehaviourTree => behaviourTree ??= GetComponent<CreatureBehaviourTree>();
        
        [SerializeField] CreatureScriptableBehaviourTree behaviourTree;
        public CreatureStatsData GetStats => _stats;

        private void Awake() => Initialise();

        private void Start() => RegisterWithManager();
        
        public NavMeshAgent GetAgent() => agent;
        public Animator GetAnimator() => animator;
        public Rigidbody GetRigidbody() => rigidBody;
        public GameObject GetModel() => model;

        #region Events

        [Space(10)]
        [Header("Events")]
        [SerializeField] public UnityEvent onFindTarget = new();
        [SerializeField] public UnityEvent onTargetFound = new();
        [SerializeField] public UnityEvent onConsumingEnter = new();
        [SerializeField] public UnityEvent onConsumingEnd = new();
        [SerializeField] public UnityEvent onDangerEnter = new();
        [SerializeField] public UnityEvent onDangerEnd = new();
        [SerializeField] public UnityEvent onStartMove = new();
        [SerializeField] public UnityEvent onTargetReached = new();
        [SerializeField] public UnityEvent onFlightEnter = new();
        [SerializeField] public UnityEvent onFlightEnd = new();
        #endregion
        
        #region Initialisation

        [ContextMenu("Initialise")]
        private void Initialise()
        {
            CheckCrucialSystems();
            
            SetupRigidbody();
            SetupCollider();
            SetupAgent();
            SetupAnimator();

            // GetBehaviourTree.Initialise();
        }

        private void SetupAnimator()
        {
            if (animator is null)
            {
                Debug.LogError($"Animator is null in {gameObject.name}");
                return;
            }
            
            animator = GetComponentInChildren<Animator>();
            model = animator.gameObject;
        }

        private void CheckCrucialSystems()
        {
            Debug.Log($"Checking crucial systems in {gameObject.name}");
            if (GetEntityData == null)
            {
                Debug.LogError($"EntityData is null in {gameObject.name}");
                return;
            }
            
            _stats ??= GetEntityData.GetStats();
            CurrentSightRange = _stats.SightRange;
            agent ??= GetComponent<NavMeshAgent>();
            rigidBody ??= GetComponent<Rigidbody>();
            capsule ??= GetComponent<CapsuleCollider>();
            // behaviourTree ??= GetComponent<CreatureBehaviourTree>();
        }
        private void SetupAgent()
        {
            if (agent is null)
            {
                Debug.LogError($"Agent is null in {gameObject.name}");
                return;
            }

            ConfigureAgent(_stats);
        }

        private void ConfigureAgent (CreatureStatsData creatureStats)
        {
            if (creatureStats is null) return;
            agent.height = creatureStats.Height;
            agent.radius = creatureStats.AvoidanceRadius;
            agent.acceleration = creatureStats.Acceleration;
            agent.speed =  creatureStats.Speed;
            agent.angularSpeed = creatureStats.AngularSpeed;
            agent.stoppingDistance = creatureStats.StoppingDistance;
        }

        private void SetupCollider()
        {
            ConfigureColliderSize(_stats);
        }

        private void ConfigureColliderSize(CreatureStatsData creatureStats)
        {
            if (creatureStats is null) return;
            capsule.direction = (int)creatureStats.CapsuleDirection; 
                
            capsule.height = creatureStats.Length;
            capsule.radius = creatureStats.Width;
        }

        private void SetupRigidbody()
        {
            if (_stats is null) return;
            rigidBody.mass = _stats.Mass;
        }
        
        private void RegisterWithManager()
        {
            // Register with manager logic
            GlobalEvents.OnRegisterCreatureEvent?.Invoke(this);
        }

        #endregion
        
        public void Move(Vector3 position, float speed = 1f)
        {
            if (_stats.MovementDefinition is not null)
            {
                switch (_stats.MovementDefinition)
                {
                    case SwimmerMovementDefinition swimmer:
                        Swim(position, swimmer.GetSpecialisedSpeed());
                        break;
                    case FlyerMovementDefinition flyer:
                        if (Vector3.Distance(transform.position, position) > _stats.SightRange / 2)
                            Fly(position, flyer.GetSpecialisedSpeed());
                        else 
                            Translate(position, speed);
                        break;
                    case JumperMovementDefinition jumper:
                        Jump();
                        break;
                    default:
                        Translate(position, speed);
                        break;
                }
            }
            else
            {
                Translate(position, speed);
            }
        }
        
        public void Translate(Vector3 position, float speed = 1f)
        {
            agent.SetDestination(position);
            agent.speed = speed;
        }
        
        public void Run(Vector3 position, float speed = 1f)
        {
            Move(position, speed * _stats.RunSpeedMultiplier);
        }
        
        [ContextMenu("Jump")]
        public void Jump()
        {
            // Because of the agent navmesh we can't jump, so we need to disable the agent, but it impacts some nodes in the behaviour tree
        }
        
        public void Swim(Vector3 position, float speed = 1f)
        {
            
        }
        
        public bool IsFlying { get; private set; } = false;

        IEnumerator FlyToPosition(Vector3 position, float speed)
        {
            Ascend();
            IsFlying = true;
            onFlightEnter?.Invoke();

            Debug.Log("Flying to position");
            Translate(position, speed);

            // Wait until the creature is close to the target position
            while (Vector3.Distance(transform.position, position) > _stats.SightRange / 2)
            {
                yield return null;
            }
            Descend();
        }

        void Fly(Vector3 position, float speed = 1f)
        {
            if (IsFlying) return;
            StartCoroutine(FlyToPosition(position, speed));
        }
        
        void Ascend()
        {
            var flyer = (FlyerMovementDefinition)_stats.MovementDefinition;
            float distanceToTarget = Vector3.Distance(transform.position, agent.destination);
            float dynamicAltitude = Mathf.Min(distanceToTarget / 2, flyer.FlightAltitude);
            float duration = CalculateDuration(dynamicAltitude, flyer.GetSpecialisedSpeed());
            model.transform.DOMoveY(model.transform.position.y + dynamicAltitude, duration)
                .SetEase(flyer.AltitudeChangeCurve);
        }

        void Descend()
        {
            var flyer = (FlyerMovementDefinition)_stats.MovementDefinition;
            float targetAltitude = agent.destination.y;
            float currentAltitude = model.transform.position.y;
            float descentDistance = currentAltitude - targetAltitude;
            float duration = CalculateDuration(descentDistance, flyer.GetSpecialisedSpeed());

            model.transform.DOMoveY(targetAltitude, duration)
                .SetEase(flyer.AltitudeChangeCurve)
                .OnComplete(() =>
                {
                    onFlightEnd?.Invoke(); 
                    IsFlying = false;
                });
        }
        
        private float CalculateDuration(float distance, float speed)
        {
            return Mathf.Abs(distance) / speed;
        }
        
        public void IncrementSightRange(BehaviourTreeContext context)
        {
            bool canIncrement = CurrentSightRangeMultiplier < CurrentMultiplierLimit;
            float increment = canIncrement ? CurrentSightRangeMultiplier += 1 : CurrentSightRangeMultiplier;
            CurrentSightRange = canIncrement ? _stats.SightRange * increment : _stats.SightRange;
            MultiplySightRange(context);
        }

        private void MultiplySightRange(BehaviourTreeContext context)
        {
            context.Creature.CurrentSightRange = context.Creature.GetStats.SightRange * context.Creature.CurrentSightRangeMultiplier;
        }
        
        public void ResetSightRange()
        {
            CurrentSightRangeMultiplier = 1;
            CurrentSightRange = _stats.SightRange;
        }

        public CreatureEntityData GetEntityData => entityData;
    }
}
