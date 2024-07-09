namespace Behaviour.Pathfinding
{
    public class Sequence : Node
    {
        public Sequence(string name, int priority = 0) : base(name, priority) {}

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
}