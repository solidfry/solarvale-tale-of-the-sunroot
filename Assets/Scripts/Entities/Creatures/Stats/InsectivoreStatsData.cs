using UnityEngine;

namespace Entities.Creatures.Stats
{
    [CreateAssetMenu(fileName = "New Insectivore Stats Data", menuName = "Entities/Creatures/Stats/Insectivore Stats Data",
        order = 0)]
    public class InsectivoreStatsData : CreatureStatsData
    {
        [field:Header("Insectivore Stats")]
        [field:SerializeField] public override FeedingBehaviourType FeedingBehaviourType { get; protected set; } = FeedingBehaviourType.Insectivore;
        [field:SerializeField] public override ActivityType ActivityType { get; protected set; } = ActivityType.Diurnal;
        [field: SerializeField] public override TimidityRating TimidityRating { get; protected set; } = TimidityRating.Timid;
        [field:SerializeField] public override MovementType MovementType { get; protected set; } = MovementType.Walker;
    }
}