using Behaviour.Pathfinding;

namespace Behaviour.ScriptableBehaviour
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "TargetInRangeNode", menuName = "Behaviours/Nodes/TargetInRangeNode")]
    public class TargetInRangeNode : ConditionNodeSo
    {
        public override NodeState Process(BehaviourTreeContext context) => 
            CheckCondition(context) ? NodeState.Success : NodeState.Failure;

        protected override bool CheckCondition(BehaviourTreeContext context)
        {
            if (context.Target == null)
            {
                return false;
            }


            float distance = Vector3.Distance(context.Agent.transform.position, context.Target.position);
            return distance <= context.Agent.stoppingDistance + context.Creature.GetStats.Width;
        }
    }
}