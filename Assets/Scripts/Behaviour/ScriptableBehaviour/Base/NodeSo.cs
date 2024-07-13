using System.Collections.Generic;
using Behaviour.Pathfinding;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.Base
{
    public abstract class NodeSo : ScriptableObject
    {
        public int priority = 0;
        public List<NodeSo> Children = new List<NodeSo>();
        [HideInInspector]
        public NodeState nodeState = NodeState.Running;

        public abstract NodeState Process(BehaviourTreeContext context);

        public virtual void Reset()
        {
            nodeState = NodeState.Running;
            foreach (var child in Children)
            {
                child.Reset();
            }
        }
        
        protected int GetCurrentChild(BehaviourTreeContext context)
        {
            return context.GetNodeChildIndex(GetInstanceID());
        }

        protected void SetCurrentChild(BehaviourTreeContext context, int index)
        {
            context.SetNodeChildIndex(GetInstanceID(), index);
        }
    }
}