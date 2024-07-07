using Entities.Creatures.Stats;
using UnityEngine;

namespace Entities.Creatures
{
    [CreateAssetMenu(fileName = "New Creature Entity Data", menuName = "Entities/Creatures/Creature Entity Data", order = 0)]
    public class CreatureEntityData : EntityData
    {
        [SerializeField] CreatureStatsData stats;
        
        public CreatureStatsData GetStats()
        {
            return stats;
        }
    }
}