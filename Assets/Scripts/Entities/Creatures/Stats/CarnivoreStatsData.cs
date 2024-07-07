using System.Collections.Generic;
using UnityEngine;

namespace Entities.Creatures.Stats
{
    [CreateAssetMenu(fileName = "New Carnivore Stats Data", menuName = "Entities/Creatures/Stats/Carnivore Stats Data", order = 0)]
    public class CarnivoreStatsData : CreatureStatsData
    {
        [field:Header("Carnivore Stats")]
        [field:SerializeField] public override FeedingBehaviourType FeedingBehaviourType { get; protected set; } = FeedingBehaviourType.Carnivore;
        [field:SerializeField] public override ActivityType ActivityType { get; protected set; } = ActivityType.Cathemeral;
        [field:SerializeField] public override MovementType MovementType { get; protected set; } = MovementType.Walker;
        [field:SerializeField] public List<CreatureEntityData> FavouritePrey { get; private set; } = new ();
    }
}