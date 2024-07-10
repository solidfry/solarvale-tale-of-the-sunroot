using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.Nodes
{
    [CreateAssetMenu(fileName = "FindPlayerNode", menuName = "Behaviours/Nodes/FindPlayerNode")]
    public class FindPlayerNodeSo : ConditionNodeSo
    {
        // ReSharper disable Unity.PerformanceAnalysis
        protected override bool CheckCondition(BehaviourTreeContext context)
        {
            if (context.Target == null)
            {
                context.SetTarget(GameObject.FindGameObjectWithTag("Player").transform);
            }

            return context.Target != null;
        }
    }
}