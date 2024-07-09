using UnityEngine;

namespace Entities.Creatures.Movement
{
    public abstract class CreatureMovementDefinition : ScriptableObject
    {
        public virtual MovementType MovementType { get; protected set; } = MovementType.Walker;
        public float Speed { get; protected set; }
        public float FastSpeedMultiplier { get; protected set; }
        public float AngularSpeed { get; protected set; }
        public float TurningSpeed { get; protected set; }
        public float Acceleration { get; protected set; }
        public float StoppingDistance { get; protected set; }
    }
}