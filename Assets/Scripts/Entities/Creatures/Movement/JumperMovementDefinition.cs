using UnityEngine;

namespace Entities.Creatures.Movement
{
    [CreateAssetMenu(fileName = "New Jumper Movement Definition", menuName = "Entities/Creatures/Movement/Jumper Movement Definition", order = 0)]
    public class JumperMovementDefinition : CreatureMovementDefinition
    {
        [field:Header("Jumper Movement Definition")]
        [field:SerializeField] public float JumpHeight { get; protected set; } = 1.0f;
        [field:SerializeField] public float JumpDistance { get; protected set; } = 1.0f;
        public override float GetSpecialisedSpeed() => Speed;
    }
}