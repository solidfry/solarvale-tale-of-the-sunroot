using Behaviour.ScriptableBehaviour;
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
        #endregion
        
        #region Initialisation

        [ContextMenu("Initialise")]
        private void Initialise()
        {
            CheckCrucialSystems();
            
            SetupRigidbody();
            SetupCollider();
            SetupAgent();
            
            // GetBehaviourTree.Initialise();
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
            agent.SetDestination(position);
            agent.speed = speed;
        }
        
        private void Run(Vector3 position, float speed = 1f)
        {
            Move(position, speed * _stats.RunSpeedMultiplier);
        }
        
        public void IncrementSightRange(BehaviourTreeContext context)
        {
            CurrentSightRange = context.Creature.CurrentSightRangeMultiplier < context.Creature.CurrentMultiplierLimit ? context.Creature.CurrentSightRangeMultiplier + 1 : context.Creature.CurrentSightRangeMultiplier;
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
