using Entities.Creatures;
using Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Plants
{
    public class Edible : Entity, IEdible
    {
        public new PlantEntityData GetEntityData => (PlantEntityData)base.GetEntityData;
        private SphereCollider col;
        public Transform GetTransform  => transform;
        [FormerlySerializedAs("isEaten")] [SerializeField] bool isConsumed = false;

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
            Debug.Log(this + " registered as an edible");
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
            if (!HasSpace) return;
            if (other.TryGetComponent(out IEntity<CreatureEntityData> entity))
            {
                if (entity.GetEntityData != null && entity.GetEntityData.GetStats().CheckIsInPreferredFood(this.GetEntityData))
                {
                    currentOccupationCount++;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out IEntity<CreatureEntityData> entity)) return;
            
            if (entity.GetEntityData != null && entity.GetEntityData.GetStats().CheckIsInPreferredFood(this.GetEntityData))
                currentOccupationCount--;
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
            col = GetComponent<SphereCollider>();

            if (GetEntityData is not null) 
                col.radius = GetEntityData.GetStats().OccupationRadius;
            else 
                col.radius = 1;
        }
        
    }
}
