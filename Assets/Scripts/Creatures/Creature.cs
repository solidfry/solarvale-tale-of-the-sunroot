using Creatures.Stats;
using Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Creatures
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody), typeof(Collider))]
    public class Creature : EntityBase<CreatureEntityData>
    {
        [field:SerializeField] public NavMeshAgent Agent { get; private set; }
        [field:SerializeField] public Rigidbody Rigidbody { get; private set; }
        [field:SerializeField] public Collider Collider { get; private set; }
        

        private void OnValidate()
        {
            if (Agent is null)
            {
                Agent = GetComponent<NavMeshAgent>();
            }

            if (Rigidbody is null)
            {
                Rigidbody = GetComponent<Rigidbody>();
            }

            if (Collider is null)
            {
                Collider = GetComponent<Collider>();
            }

            Initialise();

        }

        private void Initialise()
        {
            SetupRigidbody();
            SetupCollider();
            SetupNavMeshAgent();
            
            RegisterWithManager();
        }

        private void RegisterWithManager()
        {
            
        }

        private void SetupNavMeshAgent()
        {
            if (GetEntityData != null) Agent.acceleration = GetEntityData.Stats.Acceleration;
        }

        private void SetupCollider()
        {
        }

        private void SetupRigidbody()
        {
        }

        private void Awake()
        {
        }

        private void Start()
        {
        }


        public void Interact()
        {
        
        }
    }
}