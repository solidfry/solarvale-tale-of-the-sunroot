using Entities;
using UnityEngine;
using Utilities;
using Yarn.Unity;

namespace DialogueSystem
{
    public class SpeakingAnimationHandler : MonoBehaviour
    {
        // [SerializeField] EntityData entityData;
        enum AnimationState
        {
            Idle,
            Conversing,
            Walking
        }
    
        enum Emotion
        {
            Neutral,
            Happy,
            Concerned
        }
    
        [SerializeField] AnimationState state;
        [SerializeField] Emotion emotion;
    
        private Animator _animator;
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int IsConversing = Animator.StringToHash("IsConversing");
        private static readonly int IsHappy = Animator.StringToHash("IsHappy");
        private static readonly int IsConcerned = Animator.StringToHash("IsConcerned");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        [YarnCommand("set_animation_state")]
        public void SetAnimationState(string newState, string newEmotion)
        {
            ResetState();
            Debug.Log($"SetAnimationState: {newState} {newEmotion}");
            state = ParseState(newState);
            emotion = ParseEmotion(newEmotion);

            Debug.Log($"SetAnimationState: {state} {emotion}");
            UpdateAnimation();
        }
    
        Emotion ParseEmotion(string e) => EnumUtilities.ParseEnum<Emotion>(e);

        AnimationState ParseState(string s) => EnumUtilities.ParseEnum<AnimationState>(s);

        private void UpdateAnimation()
        {
            if (_animator.GetBool(IsMoving) && state == AnimationState.Walking) return;
            switch (state, emotion)
            {
                case (AnimationState.Idle, Emotion.Neutral):
                    SetIsConversing(false);
                    break;
                case (AnimationState.Idle, Emotion.Happy):
                    SetIsConversing(false);                
                    SetIsHappy(true);
                    break;
                case (AnimationState.Idle, Emotion.Concerned):
                    SetIsConversing(false);
                    SetIsConcerned(true);
                    break;
                case (AnimationState.Conversing, Emotion.Neutral):
                    SetIsConversing(true);
                    break;
                case (AnimationState.Conversing, Emotion.Happy):
                    SetIsConversing(true);
                    SetIsHappy(true);
                    break;
                case (AnimationState.Conversing, Emotion.Concerned):
                    SetIsConversing(true);
                    SetIsConcerned(true);
                    break;
                default:
                    break;
            }
        }
    
        private void SetIsConversing(bool value)
        {
            _animator.SetBool(IsConversing, value);
        }
    
        private void SetIsHappy(bool value)
        {
            _animator.SetBool(IsHappy, value);
        }
    
        private void SetIsConcerned(bool value)
        {
            _animator.SetBool(IsConcerned, value);
        }
    
        [YarnCommand("reset_animation_state")]
        public void ResetAnimationState()
        {
            // if (characterName != entityData.Name) return;
            ResetState();
        }

        private void ResetState()
        {
            _animator.SetBool(IsConversing, false);
            _animator.SetBool(IsHappy, false);
            _animator.SetBool(IsConcerned, false);
        }
    }
}
