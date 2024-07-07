using System.Collections.Generic;
using Entities.Plants;
using UnityEngine;

namespace Entities.Creatures.Stats
{
    [CreateAssetMenu(fileName = "New Herbivore Stats Data", menuName = "Entities/Creatures/Stats/Herbivore Stats Data",
        order = 0)]
    public class HerbivoreStatsData : CreatureStatsData
    {
        [field:Header("Herbivore Stats")]
        [field:SerializeField] public override FeedingBehaviourType FeedingBehaviourType { get; protected set; } = FeedingBehaviourType.Herbivore;
        [field:SerializeField] public override ActivityType ActivityType { get; protected set; } = ActivityType.Crepuscular;
        [field:SerializeField] public override MovementType MovementType { get; protected set; } = MovementType.Walker;
        [field: SerializeField] public List<PlantEntityData> FavouriteFood { get; private set; } = new();
    }
}