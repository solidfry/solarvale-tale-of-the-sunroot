using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.Nodes
{
    [CreateAssetMenu(fileName = "DoAudioCall", menuName = "Behaviours/Nodes/DoAudioCallNode")]
    public class  DoAudioCallSo : ConditionNodeSo
    {
        public int minCallTime = 10;
        public int maxCallTime = 30;
        protected override bool CheckCondition(BehaviourTreeContext context)
        {
            int nodeId = GetInstanceID();

            if (!context.GetNodeState(nodeId))
            {
                float timeToIdle = Random.Range(minCallTime, maxCallTime);
                context.SetNodeTimer(nodeId, timeToIdle);
                context.SetNodeState(nodeId, true);
                context.Creature.onAudioCallStart?.Invoke();
            }

            float timer = context.GetNodeTimer(nodeId);

            if (timer > 0)
            {
                timer -= Time.deltaTime;
                context.SetNodeTimer(nodeId, timer);
                return false; // Still idling
            }

            context.Creature.onAudioCallEnd?.Invoke();
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