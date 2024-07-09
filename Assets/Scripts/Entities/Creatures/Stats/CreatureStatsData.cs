using System.Collections.Generic;
using Entities.Creatures.Movement;
using Entities.Plants;
using UnityEngine;

namespace Entities.Creatures.Stats
{
    public enum DirectionAxis
    {
        X = 0,
        Y = 1,
        Z = 2
    }
    public abstract class CreatureStatsData : EntityStatsBase
    {
        
        [field: Header("Physical Attributes")]
        [field: SerializeField] public DirectionAxis CapsuleDirection { get; protected set; }
        [field: SerializeField] public float Height { get; protected set; }
        [field: SerializeField] public float Length { get; protected set; }
        [field: SerializeField] public float Width { get; protected set; }
        [field: SerializeField] public float Mass { get; protected set; }
        
        [field: Header("Navigation")]
        
        [field: SerializeField] [field:Tooltip("The movement definition for this creature. If empty the below values will be used and it will be a walking creature.")] 
        public CreatureMovementDefinition MovementDefinition { get; protected set; }
        [field: SerializeField] public float SightRange { get; protected set; }
        [field: SerializeField] public float Speed { get; protected set; }
        [field: SerializeField] public float RunSpeedMultiplier { get; protected set; }
        [field: SerializeField] public float AngularSpeed { get; protected set; }
        [field: SerializeField] public float TurningSpeed { get; protected set; }
        [field: SerializeField] public float Acceleration { get; protected set; }
        [field: SerializeField] public float StoppingDistance { get; protected set; }
        
        [field: Header("Danger")]
        [field:SerializeField] public float DangerDetectionRange { get; protected set; }
        [field:SerializeField] public float DangerFleeTime { get; protected set; } 
        
        [field:Header("Obstacle Avoidance")]
        [field: SerializeField] public float AvoidanceRadius { get; protected set; }
        [field: SerializeField] public float AvoidancePriority { get; protected set; }
        
        
        [field:Header("Feeding")]
        [field: SerializeField] public float FeedRate { get; protected set; }
        [field: SerializeField] public List<EntityData> PreferredFood { get; protected set; }
        
        
        public bool CheckIsInPreferredFood(EntityData entity) => PreferredFood.Contains(entity);
    }
}