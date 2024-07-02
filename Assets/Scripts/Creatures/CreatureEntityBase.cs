using Entities;

namespace Creatures
{
    public class CreatureEntityBase : EntityBase<CreatureEntityData>
    {
        public override CreatureEntityData GetEntityData => entityData;
    }
    
}