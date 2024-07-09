using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.Nodes
{
    [CreateAssetMenu(fileName = "TargetInRangeNode", menuName = "Behaviours/Nodes/TargetInRangeNode")]
    public class TargetInRangeNode : ConditionNodeSo
    {
        protected override bool CheckCondition(BehaviourTreeContext context)
        {
            if (context.Target == null)
            {
                return false;
            }

            float distance = Vector3.Distance(context.Agent.transform.position, context.Target.position);
            bool isInRange = distance <= context.Agent.stoppingDistance + context.Creature.GetStats.Width;
            return isInRange;
        }
    }
}