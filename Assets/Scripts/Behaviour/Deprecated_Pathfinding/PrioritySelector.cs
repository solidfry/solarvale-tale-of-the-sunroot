using System.Collections.Generic;
using System.Linq;

namespace Behaviour.Pathfinding
{
    public class PrioritySelector : Selector
    {
        List<Node> sortedChildren;
        List<Node> SortedChildren => sortedChildren ??= Sort();
        public PrioritySelector(string name, int priority = 0) : base(name, priority) {}
        
        public override void Reset()
        {
            base.Reset();
            sortedChildren = null;
        }

        protected virtual List<Node> Sort() => Children.OrderByDescending(child => child.priority).ToList();
        
        public override NodeState Process()
        {
            foreach (var child in SortedChildren)
            {
                switch (child.Process())
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Success:
                        return NodeState.Success;
                    default:
                        continue;
                }
            }
            
            
            return NodeState.Failure;
        }
    }
}