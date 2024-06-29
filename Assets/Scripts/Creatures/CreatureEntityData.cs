using Creatures.Stats;
using Entities;
using UnityEngine;

namespace Creatures
{
    [CreateAssetMenu(fileName = "New Creature Entity Data", menuName = "Entities/Creature Entity Data", order = 0)]
    public class CreatureEntityData : EntityData
    {
        [field: SerializeField] public CreatureStatsDataBase Stats { get; private set; }
        
        public CreatureStatsDataBase GetStats()
        {
            return Stats;
        }
    }
}