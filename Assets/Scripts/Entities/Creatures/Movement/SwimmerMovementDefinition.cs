using UnityEngine;

namespace Entities.Creatures.Movement
{
    [CreateAssetMenu(fileName = "New Swimmer Movement Definition", menuName = "Entities/Creatures/Movement/Swimmer Movement Definition", order = 0)]
    public class SwimmerMovementDefinition : CreatureMovementDefinition
    {
        [field:Header("Swimmer Movement Definition")]
        [field:SerializeField] public override MovementType MovementType { get; protected set; } = MovementType.Swimmer;
        [field:SerializeField] public float SwimSpeed { get; protected set; } = 1.0f;
    }
}