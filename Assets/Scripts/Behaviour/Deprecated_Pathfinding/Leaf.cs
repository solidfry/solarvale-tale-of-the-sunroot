using Utilities;

namespace Behaviour.Pathfinding
{
    public class Leaf : Node
    {
        readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy, int priority = 0) : base(name, priority)
        {
            Preconditions.CheckNotNull(strategy);
            this.strategy = strategy;
        }
        
        public override NodeState Process() => strategy.Process();
        
        public override void Reset() => strategy.Reset();
    }
}