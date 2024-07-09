using UnityEngine;

namespace Entities.Creatures.Stats
{
    [CreateAssetMenu(fileName = "New Carnivore Stats Data", menuName = "Entities/Creatures/Stats/Carnivore Stats Data", order = 0)]
    public class CarnivoreStatsData : CreatureStatsData
    {
        [field:Header("Carnivore Stats")]
        [field:SerializeField] public FeedingBehaviourType FeedingBehaviourType { get; protected set; } = FeedingBehaviourType.Carnivore;
        [field:SerializeField] public ActivityType ActivityType { get; protected set; } = ActivityType.Cathemeral;
        [field:SerializeField] public TimidityRating TimidityRating { get; protected set; } = TimidityRating.Timid;
    }
}