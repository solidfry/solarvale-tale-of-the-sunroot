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
            
            var distanceToTarget = Vector3.Distance(context.Agent.transform.position, context.Target.position);
            
            if (distanceToTarget <= context.Agent.stoppingDistance + context.Creature.GetStats.Width)
            {
                context.Agent.isStopped = true;
                return NodeState.Success;
            }
            
            if (distanceToTarget > context.Creature.GetStats.SightRange)
            {
                context.Creature.MoveFast( context.Target.position, context.Creature.GetStats.Speed);
            }
            else
            {
                context.Creature.Move(context.Target.position, context.Creature.GetStats.Speed);
            }
            
            
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