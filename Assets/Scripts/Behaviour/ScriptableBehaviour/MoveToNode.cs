using Behaviour.Pathfinding;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "MoveToTargetNode", menuName = "Behaviours/Nodes/MoveToTargetNode")]
    public class MoveToTargetNode : ConditionNodeSo
    {
        private bool hasEntered = false;
        
        [SerializeField]
        public UnityEvent onStartMove;

        [SerializeField]
        public UnityEvent onTargetReached;

        public override NodeState Process(BehaviourTreeContext context)
        {
            if (!hasEntered)
            {
                if (context.Creature.onStartMove != null)
                {
                    onStartMove = context.Creature.onStartMove;
                }
                
                if (context.Creature.onTargetReached != null)
                {
                    onTargetReached = context.Creature.onTargetReached;
                }
                
                onStartMove?.Invoke();
                hasEntered = true;
            }

            if (context.CurrentTargets.Count > 0 && context.CurrentTargets[0] != null)
            {
                context.Creature.Move(context.CurrentTargets[0].GetTransform.position, context.Creature.GetStats.Speed);
                nodeState = NodeState.Running;
            }
            else
            {
                nodeState = NodeState.Failure;
                hasEntered = false;
            }

            if (CheckCondition(context))
            {
                nodeState = NodeState.Success;
                onTargetReached?.Invoke();
                hasEntered = false;
            }

            return nodeState;
        }

        protected override bool CheckCondition(BehaviourTreeContext context)
        {
            if (context.Agent.pathStatus != NavMeshPathStatus.PathComplete) return false;
            float distanceToTarget = Vector3.Distance(context.Agent.transform.position, context.CurrentTargets[0].GetTransform.position);
            return distanceToTarget <= context.Agent.stoppingDistance + context.Creature.GetStats.Width;
        }
        
        public override void Reset()
        {
            hasEntered = false;
            onTargetReached = null;
            onStartMove = null;
        }
    }
}