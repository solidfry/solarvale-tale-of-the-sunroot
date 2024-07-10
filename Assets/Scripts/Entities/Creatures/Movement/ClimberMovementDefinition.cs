using UnityEngine;

namespace Entities.Creatures.Movement
{
    [CreateAssetMenu(fileName = "New Climber Movement Definition", menuName = "Entities/Creatures/Movement/Climber Movement Definition", order = 0)]
    public class ClimberMovementDefinition : CreatureMovementDefinition
    {
        [field:Header("Climber Movement Definition")]
        [field:SerializeField] public float ClimbSpeed { get; protected set; } = 1.0f;
        [field:SerializeField] public float ClimbAltitude { get; protected set; } = 1.0f;
        public override float GetSpecialisedSpeed() => ClimbSpeed;
    }
}