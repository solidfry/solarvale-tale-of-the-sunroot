﻿using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.Nodes
{
    [CreateAssetMenu(fileName = "DelayedActionNode", menuName = "Behaviours/Nodes/DelayedActionNode")]
    public class ConsumeActionNode : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            int nodeId = GetInstanceID();
            bool hasEntered = context.GetNodeState(nodeId);

            if (!hasEntered)
            {
                context.SetNodeState(nodeId, true);
                context.SetNodeTimer(nodeId, 0f); // Reset timer when node is entered
                context.Creature.onConsumingStart?.Invoke();
            }

            float timer = context.GetNodeTimer(nodeId);

            if (timer < context.Creature.GetStats.FeedRate)
            {
                timer += Time.deltaTime;
                context.SetNodeTimer(nodeId, timer);
                return NodeState.Running;
            }

            context.Creature.onConsumingEnd?.Invoke();
            if (context.CurrentTargets.Count > 0)
                context.CurrentTargets[0]?.Consume();

            context.SetNodeState(nodeId, false);
            ResetNode(context, nodeId);
            return NodeState.Success;
        }

        public override void Reset()
        {
            // No need to reset anything here, as we handle resetting in the context
        }

        private void ResetNode(BehaviourTreeContext context, int nodeId)
        {
            context.SetNodeState(nodeId, false);
            context.SetNodeTimer(nodeId, 0f);
        }
    }
}