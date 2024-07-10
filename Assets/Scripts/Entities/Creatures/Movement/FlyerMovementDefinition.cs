using UnityEngine;

namespace Entities.Creatures.Movement
{
    [CreateAssetMenu(fileName = "New Flyer Movement Definition", menuName = "Entities/Creatures/Movement/Flyer Movement Definition", order = 0)]
    public class FlyerMovementDefinition : CreatureMovementDefinition
    {
        [field:Header("Flyer Movement Definition")]
        [field:SerializeField] public float FlightSpeed { get; protected set; } = 1.0f;
        [field:SerializeField] public float FlightAltitude { get; protected set; } = 1.0f;
        
        [field: SerializeField] public AnimationCurve AltitudeChangeCurve { get; protected set; } = AnimationCurve.EaseInOut( 0.0f, 0.0f, 1.0f, 1.0f );
        public override float GetSpecialisedSpeed() => FlightSpeed;
    }
}