using UnityEngine;

namespace Entities.Creatures.Movement
{
    [CreateAssetMenu(fileName = "New Flier Movement Definition", menuName = "Entities/Creatures/Movement/Flier Movement Definition", order = 0)]
    public class FlierMovementDefinition : CreatureMovementDefinition
    {
        [field:Header("Flier Movement Definition")]
        [field:SerializeField] public override MovementType MovementType { get; protected set; } = MovementType.Flier;
        [field:SerializeField] public float FlightSpeed { get; protected set; } = 1.0f;
        [field:SerializeField] public float FlightAltitude { get; protected set; } = 1.0f;
    }
}