using System.Collections.Generic;
using System.Linq;
using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.CompositeNodes
{
    [CreateAssetMenu(fileName = "New Priority Selector", menuName = "Behaviours/Composite/Priority Selector", order = 2)]
    public class PrioritySelectorSo : SelectorSo
    {
        List<NodeSo> _sortedChildren;
        List<NodeSo> SortedChildren => _sortedChildren ??= Sort();
        
        protected virtual List<NodeSo> Sort() => Children.OrderByDescending(child => child.priority).ToList();
        
        public override void Reset()
        {
            base.Reset();
            _sortedChildren = null;
        }

        public override NodeState Process(BehaviourTreeContext context)
        {
            foreach (var child in SortedChildren)
            {
                switch (child.Process(context))
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