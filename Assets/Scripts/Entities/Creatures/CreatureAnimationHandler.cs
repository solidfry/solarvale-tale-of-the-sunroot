using Entities.Creatures.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.Creatures
{
    [RequireComponent(typeof(Creature))]
    public class CreatureAnimationHandler : MonoBehaviour
    {
        
        Animator _animator;
        NavMeshAgent _agent;
        Creature _creature;
        
        private static readonly int WalkingLayer = Animator.StringToHash("Walking");
        private static readonly int FlyingLayer = Animator.StringToHash("Flying");
        
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int IsSearching = Animator.StringToHash("isSearching");
        private static readonly int IsEating = Animator.StringToHash("isEating");
        private static readonly int Speed = Animator.StringToHash("speed");
        private static readonly int IsFlying = Animator.StringToHash("isFlying");
        private static readonly int IsJumping = Animator.StringToHash("isJumping");
        private static readonly int YVelocity = Animator.StringToHash("yVelocity");
        
        private Vector3 _velocity;
        private float _smoothTime = 0.2f;
        
        private void Start()
        {
            _creature = GetComponent<Creature>();
            _animator = _creature.GetAnimator();
            _agent = _creature.GetAgent();
        }

        private void Update()
        {
            _velocity = _creature.GetRigidbody().velocity;
            HandleMovementAnimations(_agent.velocity.magnitude);
            HandleRotationDuringMovement();
            SmoothModelPositionWhenFlying();
        }

        private void SmoothModelPositionWhenFlying()
        {
            if (_creature.GetStats.MovementDefinition is null || _creature is null) return;
            if (_creature.GetStats.MovementDefinition.MovementType != MovementType.Flyer) return;
            // if (!_creature.IsFlying)
            // {
            //     // Transition back to the parent's position smoothly when not flying
            //     var parentPosition = _creature.model.transform.parent.position;
            //     _creature.model.transform.position = Vector3.SmoothDamp(_creature.model.transform.position, parentPosition, ref _velocity, _smoothTime);
            //     return;
            // }
            //
            // if (Physics.Raycast( _creature.model.transform.position, Vector3.down, out var hit, 1f))
            // {
            //     var flyer = (FlyerMovementDefinition)_creature.GetStats.MovementDefinition;
            //
            //     float targetAltitude = hit.point.y + Mathf.Min(Vector3.Distance(_creature.transform.position, _creature.GetAgent().destination) / 2, flyer.FlightAltitude);
            //     Vector3 targetPosition = new Vector3(_creature.model.transform.position.x, targetAltitude, _creature.model.transform.position.z);
            //
            //     // Smoothly move the model to the target position
            //     _creature.model.transform.position = Vector3.SmoothDamp(_creature.model.transform.parent.position, targetPosition, ref _velocity, _smoothTime);
            // }
        }
        
        private void HandleMovementAnimations(float moveSpeed)
        {
            if (_animator == null) return;

            var speed = NormaliseSpeed(moveSpeed);
            SetSpeed(speed);

            if (speed > 0)
            {
                SetMoving(true);
            }
            else
            {
                SetMoving(false);
            }
        }
        
        private float NormaliseSpeed(float moveSpeed)
        {
            var maxSpeed = _creature.GetStats.Speed * _creature.GetStats.RunSpeedMultiplier;
            var normalizedSpeed = Mathf.Clamp(moveSpeed / maxSpeed, 0f, 1f);
            return normalizedSpeed;
        }
        
        private void HandleRotationDuringMovement()
        {
            if (_agent.velocity.magnitude < 0.1f) return;
            var direction = _agent.velocity.normalized;
            var lookRotation = Quaternion.LookRotation(direction);
            var rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * _creature.GetStats.TurningSpeed);
            transform.rotation = rotation;
        }
        
        public void SetSearching(bool isSearching)
        {
            _animator.SetBool(IsSearching, isSearching);
        }
        
        public void SetEating(bool isEating)
        {
            _animator.SetBool(IsEating, isEating);
        }
        
        public void SetMoving(bool isMoving)
        {
            _animator.SetBool(IsMoving, isMoving);
        }
        
        public void SetSpeed(float speed)
        {
            _animator.SetFloat(Speed, speed);
            if (_creature.IsFlying)
            {
                _animator.SetFloat(YVelocity, _velocity.y);
            }
        }
        
        public void SetFlying(bool isFlying)
        {
             
            _animator.SetBool(IsFlying, isFlying);
        }
    }
}