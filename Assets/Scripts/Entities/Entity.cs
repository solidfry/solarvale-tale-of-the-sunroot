using UnityEngine;

namespace Entities
{
    public class Entity : MonoBehaviour, IEntity<EntityData>
    {
        [SerializeField] protected EntityData entityData;
        
        public virtual EntityData GetEntityData => entityData;
    }
}