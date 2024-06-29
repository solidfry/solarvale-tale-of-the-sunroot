using UnityEngine;

namespace Entities
{
    public abstract class EntityBase<T> : MonoBehaviour, IEntity<T> where T : EntityData
    {
        [SerializeField] protected T entityData;
        
        public T GetEntityData => entityData;
    }
    
}