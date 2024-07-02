using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace Behaviour.Pathfinding
{
    public class RandomSelector : PrioritySelector
    {
        protected override List<Node> Sort() => Children.Shuffle().ToList();
        public RandomSelector(string name, int priority = 0) : base(name, priority) {}
        
    }
}