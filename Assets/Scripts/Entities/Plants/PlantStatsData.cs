using UnityEngine;

namespace Entities.Plants
{
    [CreateAssetMenu(fileName = "New Plant Stats Data", menuName = "Entities/Plants/Stats/Plant Stats Data", order = 1)]
    public class PlantStatsData : EntityStatsBase
    {
        [field: SerializeField] public float GrowthTime { get; protected set; } = 60f;
        [field: SerializeField] public int OccupationCapacity { get; protected set; } = 4;
        [field: SerializeField] public float OccupationRadius { get; protected set; } = 5f;
    }
}