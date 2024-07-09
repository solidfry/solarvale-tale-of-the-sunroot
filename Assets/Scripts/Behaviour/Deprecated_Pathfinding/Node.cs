using System.Collections.Generic;

namespace Behaviour.Pathfinding
{
    public class Node
    {
        public readonly string name;
        public readonly int priority;
        
        public readonly List<Node> Children = new();
        protected int CurrentChild;
        
        public Node(string name = "Node", int priority = 0)
        {
            this.name = name;
            this.priority = priority;
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
    
    public class Repeat : Node
    {
        public readonly int times;
        private int count;
        
        public Repeat(string name, int times, int priority = 0) : base(name, priority)
        {
            this.times = times;
        }
        
        public override NodeState Process()
        {
            if (count < times)
            {
                if (Children[0].Process() == NodeState.Success)
                {
                    count++;
                }
                
                return NodeState.Running;
            }
            
            count = 0;
            return NodeState.Success;
        }
    }
    
    public class UntilFail : Node
    {
        public UntilFail(string name, int priority = 0) : base(name, priority) {}

        public override NodeState Process()
        {
            if (Children[0].Process() == NodeState.Failure)
            {
                Reset();
                return NodeState.Failure;
            }
            
            return NodeState.Running;
        }
    }
    
    public class Inverter : Node
    {
        public Inverter(string name, int priority = 0) : base(name, priority) {}
        
        public override NodeState Process()
        {
            switch (Children[0].Process())
            {
                case NodeState.Running:
                    return NodeState.Running;
                case NodeState.Failure:
                    return NodeState.Success;
                default:
                    return NodeState.Failure;
            }
        }
    }
}