using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.CompositeNodes
{
    [CreateAssetMenu(fileName = "New Selector", menuName = "Behaviours/Composite/Selector", order = 0)]
    public class SelectorSo : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            int nodeId = GetInstanceID();
            int currentChild = context.GetNodeChildIndex(nodeId);

            while (currentChild < Children.Count)
            {
                switch (Children[currentChild].Process(context))
                {
                    case NodeState.Running:
                        context.SetNodeChildIndex(nodeId, currentChild);
                        nodeState = NodeState.Running;
                        return nodeState;
                    case NodeState.Success:
                        context.SetNodeChildIndex(nodeId, 0);
                        nodeState = NodeState.Success;
                        return nodeState;
                    default:
                        currentChild++;
                        break;
                }
            }

            context.SetNodeChildIndex(nodeId, 0);
            nodeState = NodeState.Failure;
            return nodeState;
        }

        public override void Reset()
        {
            nodeState = NodeState.Running;
            foreach (var child in Children)
            {
                child.Reset();
            }
        }
    }
}