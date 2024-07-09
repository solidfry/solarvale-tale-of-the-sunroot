using Behaviour.Pathfinding;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "IdleForSeconds", menuName = "Behaviours/Nodes/IdleForSecondsNode")]
    public class IdleForSecondsNodeSo : ConditionNodeSo
    {
        protected override bool CheckCondition(BehaviourTreeContext context)
        {
            int nodeId = GetInstanceID();

            if (!context.GetNodeState(nodeId))
            {
                float timeToIdle = Random.Range(1, 60);
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
            return true; // Idle time elapsed
        }
        
    }
}