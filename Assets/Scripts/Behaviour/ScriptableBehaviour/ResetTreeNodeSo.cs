using Behaviour.Pathfinding;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "TreeResetNode", menuName = "Behaviours/Nodes/TreeResetNode")]
    public class TreeResetNode : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            Debug.Log("Tree reset");
            return NodeState.Success;
        }
        
        public override void Reset()
        {
            Debug.Log("Tree reset");
        }
    }
}