using UnityEngine;

namespace Entities
{
    public enum EntityType
    {
        Player,
        Animal,
        Plant,
        NonPlayerCharacter,
        Prop,
        Item,
        Any
    }
    
    [CreateAssetMenu(fileName = "New Entity Data", menuName = "Entities/Entity Data", order = 0)]
    public class EntityData : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [SerializeField] private EntityType entityType;
        
        public EntityType EntityType => entityType;
    }
}