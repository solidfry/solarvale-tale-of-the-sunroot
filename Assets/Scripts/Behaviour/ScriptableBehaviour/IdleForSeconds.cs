using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "IdleForSeconds", menuName = "Behaviours/Nodes/IdleForSecondsNode")]
    public class IdleForSecondsNodeSo : ConditionNodeSo
    {
        public int minIdleTime = 3;
        public int maxIdleTime = 30;
        protected override bool CheckCondition(BehaviourTreeContext context)
        {
            int nodeId = GetInstanceID();

            if (!context.GetNodeState(nodeId))
            {
                float timeToIdle = Random.Range(minIdleTime, maxIdleTime);
                context.SetNodeTimer(nodeId, timeToIdle);
                context.SetNodeState(nodeId, true);
                Debug.Log("Idling for " + timeToIdle + " seconds");
            }

            float timer = context.GetNodeTimer(nodeId);

            if (timer > 0)
            {
                timer -= Time.deltaTime;
                context.SetNodeTimer(nodeId, timer);
                return false; // Still idling
            }

            context.SetNodeState(nodeId, false);
            ResetNode(context, nodeId);
            return true; // Idle time elapsed
        }
        
        
        private void ResetNode(BehaviourTreeContext context, int nodeId)
        {
            context.SetNodeState(nodeId, false);
            context.SetNodeTimer(nodeId, 0f);
        }
    }
}