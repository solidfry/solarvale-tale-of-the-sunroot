using Creatures.Enums;
using UnityEngine;

namespace Creatures.Stats
{
    public abstract class CreatureStatsDataBase : ScriptableObject
    {
        [field: SerializeField] public float SightRange { get; protected set; }
        
        [field: Header("Physical Attributes")]
        [field: SerializeField] public float Height { get; protected set; }
        [field: SerializeField] public float Length { get; protected set; }
        [field: SerializeField] public float Width { get; protected set; }
        [field: SerializeField] public float Mass { get; protected set; }
        
        [field: Header("Navigation")]
        [field: SerializeField] public float Speed { get; protected set; }
        [field: SerializeField] public float RunSpeedMultiplier { get; protected set; }
        [field: SerializeField] public float AngularSpeed { get; protected set; }
        [field: SerializeField] public float Acceleration { get; protected set; }
        [field: SerializeField] public float StoppingDistance { get; protected set; }
        
        [field:Header("Obstacle Avoidance")]
        [field: SerializeField] public float AvoidanceRadius { get; protected set; }
        [field: SerializeField] public float AvoidancePriority { get; protected set; }
        
        public abstract FeedingBehaviourType FeedingBehaviourType { get; protected set; }
        public abstract ActivityType ActivityType { get; protected set; }
        public abstract MovementType MovementType { get; protected set; }
    }
}