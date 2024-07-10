using UnityEngine;

namespace Entities.Creatures.Movement
{
    public abstract class CreatureMovementDefinition : ScriptableObject
    {
        public float Speed { get; protected set; }
        public float FastSpeedMultiplier { get; protected set; }
        public float AngularSpeed { get; protected set; }
        public float TurningSpeed { get; protected set; }
        public float Acceleration { get; protected set; }
        public float StoppingDistance { get; protected set; }
        
        public abstract float GetSpecialisedSpeed();
        public virtual float GetSpeed() => Speed;
    }
}