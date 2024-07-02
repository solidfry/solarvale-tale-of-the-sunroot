using System.Collections.Generic;
using Entities;
using UnityEngine;

namespace Creatures.Stats
{
    [CreateAssetMenu(fileName = "New Omnivore Stats Data", menuName = "Entities/Stats/Omnivore Stats Data", order = 0)]
    public class OmnivoreStatsData : CreatureStatsDataBase
    {
        [field:Header("Omnivore Stats")]
        [field: SerializeField] public override FeedingBehaviourType FeedingBehaviourType { get; protected set; } = FeedingBehaviourType.Omnivore;
        [field:SerializeField] public override ActivityType ActivityType { get; protected set; } = ActivityType.Diurnal;
        [field:SerializeField] public override MovementType MovementType { get; protected set; } = MovementType.Walker;
        [field: SerializeField] public List<CreatureEntityData> FavouritePrey { get; private set; } = new();
        [field: SerializeField] public List<PlantEntityData> FavouriteFood { get; private set; } = new();
    }
}