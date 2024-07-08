using Behaviour.Pathfinding;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "New Sequence", menuName = "Behaviours/Composite/Sequence", order = 0)]
    public class SequenceSo : NodeSo
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
                    case NodeState.Failure:
                        nodeState = NodeState.Failure;
                        return nodeState;
                    default:
                        CurrentChild++;
                        return CurrentChild == Children.Count ? NodeState.Success : NodeState.Running;
                }
            }
            
            Reset();
            nodeState = NodeState.Success;
            return nodeState;
        }
    }
}