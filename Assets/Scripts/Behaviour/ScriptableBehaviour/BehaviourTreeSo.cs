using Behaviour.Pathfinding;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "BehaviourTree", menuName = "Behaviours/BehaviourTree")]
    public class BehaviourTreeSo : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            while (CurrentChild < Children.Count)
            {
                var status = Children[CurrentChild].Process(context);
                if (status != NodeState.Success)
                {
                    return status;
                }
                CurrentChild++;
            }
            return NodeState.Success;
        }
        
        public override void Reset()
        {
            CurrentChild = 0;
            foreach (var child in Children)
            {
                child.Reset();
            }
        }
    }
}