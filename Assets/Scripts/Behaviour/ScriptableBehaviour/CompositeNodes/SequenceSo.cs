using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.CompositeNodes
{
    [CreateAssetMenu(fileName = "SequenceNode", menuName = "Behaviours/Composite/Sequence")]
    public class SequenceNode : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            int currentChild = GetCurrentChild(context);

            while (currentChild < Children.Count)
            {
                NodeState childState = Children[currentChild].Process(context);
                if (childState == NodeState.Running)
                {
                    SetCurrentChild(context, currentChild);
                    return NodeState.Running;
                }
                if (childState == NodeState.Failure)
                {
                    SetCurrentChild(context, 0); // Reset for next execution
                    return NodeState.Failure;
                }
                currentChild++;
            }

            SetCurrentChild(context, 0); // Reset for next execution
            return NodeState.Success;
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}