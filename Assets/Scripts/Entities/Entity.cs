using UnityEngine;

namespace Entities
{
    public class Entity : MonoBehaviour, IEntity
    {
        [SerializeField] private EntityData entityData;
        
        public EntityData GetEntityData => entityData;
    }
}