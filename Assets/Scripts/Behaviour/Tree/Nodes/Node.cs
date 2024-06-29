using System.Collections.Generic;
using UnityEngine;

namespace Behaviour.Tree.Nodes
{
    public enum NodeState
    {
        Running,
        Failure,
        Success,
    }

    public class Node
    {
        protected NodeState State;
        
        public Node Parent { get; private set; }

        protected List<Node> Children;

        private Dictionary<string, object> _dataContext = new();

        public Node()
        {
            Parent = null;
        }

        protected Node(List<Node> children)
        {
            foreach (var child in children)
                Attach(child);
        }

        protected Node GetRootNode()
        {
            Node node = this;
            while (node.Parent is not null)
            {
                node = node.Parent;
            }

            return node;
        }

        private void Attach(Node node)
        {
            node.Parent = this;
            if (Children is null)
            {
                Children = new List<Node>();
            }
            Debug.Log(node + $" failed at {node.State}" + " attached to " + this + " and the parent is " + node.Parent + " and the children are " + Children);
            Children.Add(node);
        }

        public virtual NodeState Evaluate()
        {
            return NodeState.Failure;
        }

        public void SetData(string key, object value)
        {
            _dataContext[key] = value;
        }

        public object GetData(string key)
        {
            if (_dataContext.TryGetValue(key, out var value))
            {
                return value;
            }

            Node node = Parent;

            while (node is not null)
            {
                value = node.GetData(key);
                if (value is not null)
                {
                    return value;
                }
                node = node.Parent;
            }
            return null;
        }

        public bool ClearData(string key)
        {
            if (_dataContext.ContainsKey(key))
            {
                _dataContext.Remove(key);
                return true;
            }
            
            Node node = Parent;
            
            while (node is not null)
            {
                bool cleared = node.ClearData(key);
                if (cleared)
                {
                    return true;
                }
                node = node.Parent;
            }

            return false;
        }
        
        
    }
}