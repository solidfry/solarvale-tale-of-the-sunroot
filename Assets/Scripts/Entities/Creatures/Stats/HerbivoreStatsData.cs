using UnityEngine;

namespace Entities.Creatures.Stats
{
    [CreateAssetMenu(fileName = "New Herbivore Stats Data", menuName = "Entities/Creatures/Stats/Herbivore Stats Data",
        order = 0)]
    public class HerbivoreStatsData : CreatureStatsData
    {
        [field:Header("Herbivore Stats")]
        [field:SerializeField] public FeedingBehaviourType FeedingBehaviourType { get; protected set; } = FeedingBehaviourType.Herbivore;
        [field:SerializeField] public ActivityType ActivityType { get; protected set; } = ActivityType.Crepuscular;
        [field:SerializeField] public TimidityRating TimidityRating { get; protected set; } = TimidityRating.Timid;
    }
}