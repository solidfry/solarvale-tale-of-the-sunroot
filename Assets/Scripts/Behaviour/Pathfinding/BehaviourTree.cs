
namespace Behaviour.Pathfinding
{
    public class BehaviourTree : Node
    {
        public BehaviourTree(string name) : base(name) {}
        
        public override NodeState Process()
        {
            while(CurrentChild < Children.Count)
            {
                var status = Children[CurrentChild].Process();
                
                if (status != NodeState.Success) 
                    return status;
                
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