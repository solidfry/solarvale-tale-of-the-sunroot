using Entities.Creatures;
using Events;
using UnityEngine;

namespace Entities.Plants
{
    [RequireComponent(typeof(SphereCollider))]
    public class Edible : Entity, IEdible
    {
        public new PlantEntityData GetEntityData => (PlantEntityData)base.GetEntityData;
        [SerializeField] private SphereCollider sphereCollider;
        public Transform GetTransform  => transform;
        [SerializeField] bool isConsumed = false;

        [SerializeField] bool isOccupied;
        [SerializeField] private int currentOccupationCount;

        private void Awake()
        {
            currentOccupationCount = 0;
            SetColliderRadius();
        }
        
        private void Start()
        {
            GlobalEvents.OnRegisterEdibleEvent?.Invoke(this);
            // Debug.Log(this + " registered as an edible");
        }
        
        public void OnDisable()
        {
            GlobalEvents.OnDeregisterEdibleEvent?.Invoke(this);
        }

        public bool IsOccupied
        {
            get
            {
                isOccupied = !HasSpace;
                return isOccupied;
            }
        }
        
        bool HasSpace => currentOccupationCount < GetEntityData?.GetStats().OccupationCapacity;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!HasSpace || IsOccupied) return;
            if (other.TryGetComponent(out IEntity<CreatureEntityData> entity))
            {
                // Debug.Log("Creature is in the trigger");
                if (entity.GetEntityData != null)
                {
                    // Debug.Log("Creature is in the trigger and is in preferred food");
                    currentOccupationCount++;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IEntity<CreatureEntityData> entity))
            {
                if (entity.GetEntityData != null)
                    currentOccupationCount--;
            }
        }

        public void Consume()
        {
            if (IsConsumed) return;
            isConsumed = true;
            GlobalEvents.OnSendEdibleEatenEvent?.Invoke(this);
        }
        
        public bool IsConsumed => isConsumed;
        
        public void Reset()
        {
            isConsumed = false;
            currentOccupationCount = 0;
        }
        
        private void SetColliderRadius()
        {
            sphereCollider ??= GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;

            if (GetEntityData is not null) 
                sphereCollider.radius = GetEntityData.GetStats().OccupationRadius;
            else 
                sphereCollider.radius = 1;
        }
        
        [ContextMenu("Initialise")]
        private void Initialise()
        {
            SetColliderRadius();
        }

        private void OnValidate()
        {
            SetColliderRadius();
        }
        
    }
}
