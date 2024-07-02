using System.Collections.Generic;
using UnityEngine;

namespace Creatures.Stats
{
    [CreateAssetMenu(fileName = "New Insectivore Stats Data", menuName = "Entities/Stats/Insectivore Stats Data",
        order = 0)]
    public class InsectivoreStatsData : CreatureStatsDataBase
    {
        [field:Header("Insectivore Stats")]
        [field:SerializeField] public override FeedingBehaviourType FeedingBehaviourType { get; protected set; } = FeedingBehaviourType.Insectivore;
        [field:SerializeField] public override ActivityType ActivityType { get; protected set; } = ActivityType.Diurnal;
        [field:SerializeField] public override MovementType MovementType { get; protected set; } = MovementType.Walker;

        [field: SerializeField] public List<CreatureEntityData> FavouritePrey { get; private set; } = new();
    }
}