using UnityEngine;

namespace Entities
{
    [CreateAssetMenu(fileName = "New Entity Data", menuName = "Entities/Entity Data", order = 0)]
    public class EntityData : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; }
        [SerializeField] private EntityType entityType;
        public static EntityData Empty
        {
            get
            {
                var empty = CreateInstance<EntityData>();
                empty.Name = "Empty";
                empty.entityType = EntityType.None;
                return empty;
            }
        }

        public EntityType EntityType => entityType;
    }
}