using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;
using UnityEngine.AI;

namespace Behaviour.ScriptableBehaviour.Nodes
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
                context.Creature.onMoveStart?.Invoke();
                context.SetNodeState(nodeId, true);
            }

            if (context.CurrentTargets.Count > 0 && context.CurrentTargets[0] != null)
            {
                // Debug.Log(context.Agent);
                if (context.Agent.isStopped)
                {
                    context.Agent.isStopped = false;
                }
                
                var distanceToTarget = Vector3.Distance(context.Agent.transform.position, context.Target.position);
                
                if (distanceToTarget > context.Creature.GetStats.SightRange)
                {
                    context.Creature.MoveFast( context.Target.position, context.Creature.GetStats.Speed);
                }
                else
                {
                    context.Creature.Move(context.Target.position, context.Creature.GetStats.Speed);
                }
                
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
                context.Creature.onMoveEnd?.Invoke();
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
