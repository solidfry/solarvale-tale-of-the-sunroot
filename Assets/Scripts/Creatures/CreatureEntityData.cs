using Creatures.Stats;
using Entities;
using UnityEngine;

namespace Creatures
{
    [CreateAssetMenu(fileName = "New Creature Entity Data", menuName = "Entities/Creature Entity Data", order = 0)]
    public class CreatureEntityData : EntityData
    {
        [SerializeField] public CreatureStatsDataBase stats;
        
        public CreatureStatsDataBase GetStats()
        {
            return stats;
        }
    }
}