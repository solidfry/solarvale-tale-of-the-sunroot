using System.Collections.Generic;
using Behaviour.Tree.Nodes;
using Utilities;

namespace Behaviour.Pathfinding
{
    public class Sequence : Node
    {
        public Sequence(string name) : base(name) {}

        public override NodeState Process()
        {
            if (CurrentChild < Children.Count)
            {
                switch (Children[CurrentChild].Process())
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Failure:
                        Reset();
                        return NodeState.Failure;
                    default:
                        CurrentChild++;
                        return CurrentChild == Children.Count ? NodeState.Success : NodeState.Running;
                }
            }
            
            Reset();
            return NodeState.Success;
        }
    }

    public class Selector : Node
    {
        public Selector(string name) : base(name) {}
        
        public override NodeState Process()
        {
            if (CurrentChild < Children.Count)
            {
                switch (Children[CurrentChild].Process())
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Success:
                        Reset();
                        return NodeState.Success;
                    default:
                        CurrentChild++;
                        return NodeState.Running;
                    
                }
            }
            
            Reset();
            return NodeState.Failure;
        }
        
    }
    
    public class Leaf : Node
    {
        readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy) : base(name)
        {
            Preconditions.CheckNotNull(strategy);
            this.strategy = strategy;
        }
        
        public override NodeState Process() => strategy.Process();
        
        public override void Reset() => strategy.Reset();
    }

    public class Node
    {
        public readonly string name;
        
        public readonly List<Node> Children = new();
        protected int CurrentChild;
        
        public Node(string name = "Node")
        {
            this.name = name;
        }
        
        public void AddChild(Node child) => Children.Add(child);
        
        public virtual NodeState Process() => Children[CurrentChild].Process();
        
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