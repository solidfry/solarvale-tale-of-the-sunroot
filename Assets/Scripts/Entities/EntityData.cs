using Creatures.Enums;
using UnityEngine;

namespace Entities
{
    [CreateAssetMenu(fileName = "New Entity Data", menuName = "Entities/Entity Data", order = 0)]
    public class EntityData : ScriptableObject
    {
        [field: SerializeField] public string EntityName { get; private set; }
        [field: SerializeField] public EntityType EntityType { get; protected set; } = EntityType.NonPlayerCharacter;
        public static EntityData Empty
        {
            get
            {
                var empty = CreateInstance<EntityData>();
                empty.EntityName = "";
                empty.EntityType = EntityType.None;
                return empty;
            }
        }
    }
}