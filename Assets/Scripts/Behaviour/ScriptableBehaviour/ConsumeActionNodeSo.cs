using Behaviour.Pathfinding;
using UnityEngine;
using UnityEngine.Events;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "DelayedActionNode", menuName = "Behaviours/Nodes/DelayedActionNode")]
    public class ConsumeActionNode : NodeSo
    {
        private float timer;
        private bool hasEntered = false;
        
        public UnityEvent doWhileDelayingAction;
        public UnityEvent doAfterDelayingAction;
        
        public override NodeState Process(BehaviourTreeContext context)
        {
            if (!hasEntered)
            {
                hasEntered = true; 
                
                if (context.Creature.onConsumingEnter != null)
                {
                    doWhileDelayingAction = context.Creature.onConsumingEnter;
                }
                
                if (context.Creature.onConsumingEnd != null)
                {
                    doAfterDelayingAction = context.Creature.onConsumingEnd;
                }

                timer = 0f; // Reset timer when node is entered
            }

            if (timer < context.Creature.GetStats.FeedRate)
            {
                timer += Time.deltaTime;
                // doWhileDelayingAction?.Invoke();
                context.Creature.onConsumingEnter?.Invoke();
                return NodeState.Running;
            }

            context.Creature.onConsumingEnd?.Invoke();
            if (context.CurrentTargets.Count > 0)
                context.CurrentTargets[0]?.Consume();
            
            hasEntered = false;
            Reset();
            return NodeState.Success;
        }
        
        public override void Reset()
        {
            hasEntered = false;
            timer = 0f;
        }
    }
    
}