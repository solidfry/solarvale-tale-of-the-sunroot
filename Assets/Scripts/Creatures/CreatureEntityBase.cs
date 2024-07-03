using Entities;
using UnityEngine;

namespace Creatures
{
    public class CreatureEntityBase : MonoBehaviour, IEntity<CreatureEntityData>
    {
        public CreatureEntityData GetEntityData { get; }
    }
    
}