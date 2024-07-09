using Behaviour.Pathfinding;

namespace Behaviour.ScriptableBehaviour
{
    public abstract class ConditionNodeSo : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            NodeState result = CheckCondition(context) ? NodeState.Success : NodeState.Failure;
            nodeState = result;
            return result;
        }

        protected abstract bool CheckCondition(BehaviourTreeContext context);
    }
}