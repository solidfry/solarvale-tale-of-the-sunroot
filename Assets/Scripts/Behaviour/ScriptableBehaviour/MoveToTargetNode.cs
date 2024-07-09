using Behaviour.Pathfinding;
using UnityEngine;
using UnityEngine.AI;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "MoveToTargetNode", menuName = "Behaviours/Nodes/MoveToTargetNode")]
    public class MoveToTargetNode : ConditionNodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            int nodeId = GetInstanceID();
            bool hasEntered = context.GetNodeState(nodeId);

            if (!hasEntered)
            {
                context.Creature.onStartMove?.Invoke();
                context.SetNodeState(nodeId, true);
            }

            if (context.CurrentTargets.Count > 0 && context.CurrentTargets[0] != null)
            {
                if (context.Agent.isStopped)
                {
                    context.Agent.isStopped = false;
                }
                
                context.Creature.Move(context.CurrentTargets[0].GetTransform.position, context.Creature.GetStats.Speed);
                nodeState = NodeState.Running;
            }
            else
            {
                nodeState = NodeState.Failure;
                context.SetNodeState(nodeId, false);
            }

            if (CheckCondition(context))
            {
                nodeState = NodeState.Success;
                context.Creature.onTargetReached?.Invoke();
                context.Agent.isStopped = true;
                context.SetNodeState(nodeId, false);
            }

            if (context.Agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                nodeState = NodeState.Failure;
                context.SetNodeState(nodeId, false);
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
            // No need to reset hasEntered here, as it is managed by the context
        }
    }
}
