using Events;
using UnityEngine;

namespace Entities.Plants
{
    public class Edible : Entity, IEdible
    {
        public new PlantEntityData GetEntityData => (PlantEntityData)base.GetEntityData;
        public Transform GetTransform  => transform;
        [SerializeField] bool isEaten = false;
        
        public void Consume()
        {
            isEaten = true;
            GlobalEvents.OnSendEdibleEatenEvent?.Invoke(this);
        }

        public bool IsConsumed => isEaten;
        
        public void Reset()
        {
            isEaten = false;
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
    }
}
