using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "BehaviourTree", menuName = "BehaviourTree/BehaviourTree")]
    public class BehaviourTreeSo : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            return Children[0].Process(context); // Assuming the first child is the root node
        }
    }
}