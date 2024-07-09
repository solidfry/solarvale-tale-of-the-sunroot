using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "BehaviourTree", menuName = "Behaviours/Tree", order = 0)]
    public class BehaviourTreeSo : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            return Children[0].Process(context); // Assuming the first child is the root node
        }
    }
}