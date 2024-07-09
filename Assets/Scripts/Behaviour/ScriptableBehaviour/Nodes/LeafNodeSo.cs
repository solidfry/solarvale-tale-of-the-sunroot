using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using Behaviour.ScriptableBehaviour.Strategy;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.Nodes
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