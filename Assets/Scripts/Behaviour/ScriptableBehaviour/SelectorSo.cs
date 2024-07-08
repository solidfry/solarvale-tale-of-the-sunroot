using Behaviour.Pathfinding;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "New Selector", menuName = "Behaviours/Composite/Selector", order = 0)]
    public class SelectorSo : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            if (CurrentChild < Children.Count)
            {
                switch (Children[CurrentChild].Process(context))
                {
                    case NodeState.Running:
                        nodeState = NodeState.Running;
                        return nodeState;
                    case NodeState.Success:
                        Reset();
                        nodeState = NodeState.Success;
                        return nodeState;
                    default:
                        CurrentChild++;
                        return NodeState.Running;
                }
            }
            
            Reset();
            nodeState = NodeState.Failure;
            return nodeState;
        }
    }
}