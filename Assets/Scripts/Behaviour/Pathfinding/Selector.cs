namespace Behaviour.Pathfinding
{
    public class Selector : Node
    {
        public Selector(string name, int priority = 0) : base(name, priority) {}
        
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
}