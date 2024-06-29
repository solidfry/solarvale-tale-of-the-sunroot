using System.Collections.Generic;
using Entities;
using UnityEngine;

namespace Creatures.Stats
{
    [CreateAssetMenu(fileName = "New Herbivore Stats Data", menuName = "Entities/Stats/Herbivore Stats Data",
        order = 0)]
    public class HerbivoreStatsData : CreatureStatsDataBase
    {
        [field:Header("Herbivore Stats")]
        [field:SerializeField] public override FeedingBehaviourType FeedingBehaviourType { get; protected set; } = FeedingBehaviourType.Herbivore;
        [field:SerializeField] public override ActivityType ActivityType { get; protected set; } = ActivityType.Crepuscular;
        [field:SerializeField] public override MovementType MovementType { get; protected set; } = MovementType.Walker;
        [field: SerializeField] public List<PlantEntityData> FavouriteFood { get; private set; } = new();
    }
}