using Entities.Creatures.Stats;
using Events;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.Creatures
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Creature : Entity
    {
        [SerializeField] Rigidbody rigidBody;
        [SerializeField] NavMeshAgent agent;
        [SerializeField] CapsuleCollider capsule;
        [SerializeField] Animator animator;
        
        [SerializeField] CreatureBehaviourTree behaviourTree;
        public CreatureBehaviourTree GetBehaviourTree => behaviourTree ??= GetComponent<CreatureBehaviourTree>();
        
        CreatureStatsData _stats;
        
        public CreatureStatsData GetStats => _stats;

        private void Awake() => Initialise();

        private void Start() => RegisterWithManager();
        
        public NavMeshAgent GetAgent() => agent;
        public Animator GetAnimator() => animator;
        
        #region Initialisation

        [ContextMenu("Initialise")]
        private void Initialise()
        {
            CheckCrucialSystems();
            
            SetupRigidbody();
            SetupCollider();
            SetupAgent();
            
            GetBehaviourTree.Initialise();
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
            agent ??= GetComponent<NavMeshAgent>();
            rigidBody ??= GetComponent<Rigidbody>();
            capsule ??= GetComponent<CapsuleCollider>();
            behaviourTree ??= GetComponent<CreatureBehaviourTree>();
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

        public new CreatureEntityData GetEntityData => entityData as CreatureEntityData;
    }
}
