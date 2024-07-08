using System.Collections.Generic;
using Behaviour.Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

namespace Behaviour.ScriptableBehaviour
{
    public abstract class NodeSo : ScriptableObject
    {
        public List<NodeSo> Children = new();
        protected int CurrentChild = 0;
        
        [FormerlySerializedAs("state")] [HideInInspector]
        public NodeState nodeState = NodeState.Running;
        
        public virtual NodeState Process(BehaviourTreeContext context)
        {
            if (Children.Count == 0)
            {
                return NodeState.Failure; // or Success, depending on your logic
            }

            while (CurrentChild < Children.Count)
            {
                var status = Children[CurrentChild].Process(context);
                if (status == NodeState.Running || status == NodeState.Failure)
                {
                    return status;
                }
                CurrentChild++;
            }

            return NodeState.Success;
        }

        public virtual void Reset()
        {
            CurrentChild = 0;
            foreach (var child in Children)
            {
                child.Reset();
            }
        }
        
    }
}