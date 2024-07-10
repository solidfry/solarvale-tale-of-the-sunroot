using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;
using UnityEngine.AI;

namespace Behaviour.ScriptableBehaviour.Nodes
{
    [CreateAssetMenu(fileName = "FollowTargetNode", menuName = "Behaviours/Nodes/FollowTargetNode")]
    public class FollowTargetNodeSo : ConditionNodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            if (context.Agent == null)
            {
                return NodeState.Failure;
            }
            
            if (context.Target == null)
            {
                return NodeState.Failure;
            }
            
            if (context.Agent.isStopped)
            {
                context.Agent.isStopped = false;
            }
            
            if (Vector3.Distance(context.Agent.transform.position, context.Target.position) <= context.Agent.stoppingDistance + context.Creature.GetStats.Width)
            {
                context.Agent.isStopped = true;
                return NodeState.Success;
            }
            
            context.Creature.Move(context.Target.position, context.Creature.GetStats.Speed);
            // Debug.Log("Following target " + context.Target.name);
            
            
            return NodeState.Running;
        }
        
        protected override bool CheckCondition(BehaviourTreeContext context)
        {
            if (context.Agent.pathStatus != NavMeshPathStatus.PathComplete) return false;
            float distanceToTarget = Vector3.Distance(context.Agent.transform.position, context.Target.position);
            return distanceToTarget <= context.Agent.stoppingDistance + context.Creature.GetStats.Width;
        }
    }
}