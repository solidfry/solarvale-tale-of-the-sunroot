using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Strategy;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    public class LeafNodeSo : NodeSo
    {
        [SerializeField] ScriptableStrategySo strategy;
        public override NodeState Process(BehaviourTreeContext context) => strategy.Process(context);
        
        public override void Reset()
        {
            strategy.Reset();
        }
    }
}