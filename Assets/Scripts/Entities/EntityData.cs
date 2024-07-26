using JetBrains.Annotations;
using UnityEngine;

namespace Entities
{
    [CreateAssetMenu(fileName = "New Entity Data", menuName = "Entities/Entity Data", order = 0)]
    public class EntityData : ScriptableObject
    {
        [SerializeField] private Sprite avatar;
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

        public virtual EntityType EntityType => entityType;
        
        [CanBeNull] public Sprite Avatar => avatar;
    }
}