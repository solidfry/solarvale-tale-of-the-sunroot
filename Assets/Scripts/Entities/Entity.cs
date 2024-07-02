using UnityEngine;

namespace Entities
{
    public class Entity : MonoBehaviour, IEntity<EntityData>
    {
        [SerializeField] private EntityData entityData;
        
        public EntityData GetEntityData => entityData;
    }
}