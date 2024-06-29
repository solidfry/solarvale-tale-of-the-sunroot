using System.Collections.Generic;

namespace Behaviour.Tree.Nodes
{
    public class Sequence : Node
    {
        
        public Sequence() : base() {}
        public Sequence(List<Node> children) : base(children) {}
        
        public override NodeState Evaluate()
        {
            bool anyChildRunning = false;

            foreach (var node in Children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.Failure:
                        State = NodeState.Failure;
                        return State;
                    case NodeState.Success:
                        continue;
                    case NodeState.Running:
                        anyChildRunning = true;
                        continue;
                    default:
                        State = NodeState.Success;
                        return State;
                }
            }
            
            State = anyChildRunning ? NodeState.Running : NodeState.Success;
            return State;
        }
    }
}