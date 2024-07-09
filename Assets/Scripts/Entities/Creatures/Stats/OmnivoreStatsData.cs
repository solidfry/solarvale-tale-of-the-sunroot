using UnityEngine;

namespace Entities.Creatures.Stats
{
    [CreateAssetMenu(fileName = "New Omnivore Stats Data", menuName = "Entities/Creatures/Stats/Omnivore Stats Data", order = 0)]
    public class OmnivoreStatsData : CreatureStatsData
    {
        [field:Header("Omnivore Stats")]
        [field:SerializeField] public FeedingBehaviourType FeedingBehaviourType { get; protected set; } = FeedingBehaviourType.Omnivore;
        [field:SerializeField] public ActivityType ActivityType { get; protected set; } = ActivityType.Diurnal;
        [field:SerializeField] public TimidityRating TimidityRating { get; protected set; } = TimidityRating.Timid;
    }
}