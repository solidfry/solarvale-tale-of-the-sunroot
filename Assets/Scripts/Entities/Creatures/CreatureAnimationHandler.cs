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