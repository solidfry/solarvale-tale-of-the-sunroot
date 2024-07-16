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
        
        CreatureStatsData _stats;
        [SerializeField] CreatureScriptableBehaviourTree behaviourTree;
        public CreatureScriptableBehaviourTree GetBehaviourTree => behaviourTree ??= GetComponent<CreatureScriptableBehaviourTree>();
        public CreatureStatsData GetStats => _stats;
        public NavMeshAgent GetAgent() => agent;
        public Animator GetAnimator() => animator;
        public Rigidbody GetRigidbody() => rigidBody;
        public GameObject GetModel() => model;

        #region Events

        [Space(10)]
        [Header("Events")]
        [SerializeField] public UnityEvent onFindTargetStart = new();
        [SerializeField] public UnityEvent onFindTargetEnd = new();
        [SerializeField] public UnityEvent onConsumingStart = new();
        [SerializeField] public UnityEvent onConsumingEnd = new();
        [SerializeField] public UnityEvent onDangerStart = new();
        [SerializeField] public UnityEvent onDangerEnd = new();
        [SerializeField] public UnityEvent onMoveStart = new();
        [SerializeField] public UnityEvent onMoveEnd = new();
        [SerializeField] public UnityEvent onFlightStart = new();
        [SerializeField] public UnityEvent onFlightEnd = new();
        [SerializeField] public UnityEvent onAudioCallStart = new();
        [SerializeField] public UnityEvent onAudioCallEnd = new();
        #endregion
        
        private void Awake() => Initialise();

        private void Start() => RegisterWithManager();
        
        #region Initialisation

        [ContextMenu("Initialise")]
        private void Initialise()
        {
            CheckCrucialSystems();
            
            SetupRigidbody();
            SetupCollider();
            SetupAgent();
            SetupAnimator();
        }

        private void SetupAnimator()
        {
            if (animator is null)
            {
                Debug.LogError($"Animator is null in {gameObject.name}");
                return;
            }
            
            animator = GetComponentInChildren<Animator>();
            if (model is null)
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
            behaviourTree ??= GetComponent<CreatureScriptableBehaviourTree>();
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
                        Swim(position, swimmer);
                        break;
                    case FlyerMovementDefinition flyer:
                        if (Vector3.Distance(transform.position, position) > _stats.SightRange / 2)
                            Fly(position, flyer);
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
        
        public void Swim(Vector3 position, SwimmerMovementDefinition swimmer)
        {
            
        }
        
        public bool IsFlying { get; private set; } = false;

        IEnumerator FlyToPosition(Vector3 position, FlyerMovementDefinition flyer)
        {
            Ascend(position, flyer);
            IsFlying = true;
            onFlightStart?.Invoke();

            Translate(position, flyer.GetSpecialisedSpeed());

            while (Vector3.Distance(transform.position, position) > _stats.SightRange / 2)
            {
                yield return null;
            }
            Descend(position, flyer);
        }

        void Fly(Vector3 position, FlyerMovementDefinition flyer)
        {
            if (IsFlying) return;
            StartCoroutine(FlyToPosition(position, flyer));
        }
        
        void Ascend(Vector3 position, FlyerMovementDefinition flyer)
        {
            float distanceToTarget = Vector3.Distance(transform.position, position);
            float dynamicAltitude = Mathf.Min(distanceToTarget / 2, flyer.FlightAltitude);
            float duration = CalculateDuration(dynamicAltitude, flyer.GetSpecialisedSpeed());
            
            Vector3 targetPosition = new Vector3(model.transform.position.x, model.transform.position.y + dynamicAltitude, model.transform.position.z);
            model.transform.DOMoveY(targetPosition.y, duration).SetEase(flyer.AltitudeChangeCurve);
        }

        void Descend(Vector3 position, FlyerMovementDefinition flyer)
        {
            float targetAltitude = transform.position.y;
            float currentAltitude = model.transform.position.y;
            float descentDistance = currentAltitude - targetAltitude;
            float duration = CalculateDuration(descentDistance, flyer.GetSpecialisedSpeed());
            
            Vector3 targetPosition = new Vector3(position.x, targetAltitude, position.z);
            model.transform.DOMoveY(targetPosition.y, duration).OnComplete(() =>
            {
                onFlightEnd?.Invoke();
                IsFlying = false;
                model.transform.localPosition = Vector3.zero;
            });
        }
        
        private float CalculateDuration(float distance, float speed) => Mathf.Abs(distance) / speed;

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
