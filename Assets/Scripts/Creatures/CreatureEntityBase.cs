using Entities;
using UnityEngine;

namespace Creatures
{
    public class CreatureEntityBase : EntityBase<CreatureEntityData>
    {
        [SerializeField] protected new CreatureEntityData entityData;
        
        public new CreatureEntityData GetEntityData => entityData;
    }
    
}