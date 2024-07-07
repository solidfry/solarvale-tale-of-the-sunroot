using UnityEngine;

namespace Entities.Plants
{
    [CreateAssetMenu(fileName = "New Plant Entity Data", menuName = "Entities/Plants/Plant Entity Data", order = 1)]
    public class PlantEntityData : EntityData
    {
        [field:Header("Plant Entity Data")]
        public override EntityType EntityType { get; } = EntityType.Plant;
        
        [SerializeField] PlantStatsData stats;
        
        public PlantStatsData GetStats()
        {
            return stats;
        }
    }

    
}