using System;
using System.Linq;
using Behaviour.Tree.Nodes;
using UnityEngine;

namespace Creatures.Behaviours
{
    public class TaskAction : Node
    {
        Creature _creature;
        Action _action;
        private float _actionTime = 2f;
        private float _actionWaitCounter = 0f;
        private bool _isPerformingAction = true;

        public TaskAction(Creature creature, Action action)
        {
            _creature = creature;
            _action = action;
            _actionTime = creature.GetStats.FeedRate;
        }
        
        public override NodeState Evaluate()
        {
            if (_creature is null || _action is null || _creature.GetBehaviourTree.GetFoodSourcesAsRef().All(x => x is null))
            {
                State = NodeState.Failure;
                Debug.Log("TaskAction: Failure  - Creature, action or target is null");
                return State;
            }

            if (_isPerformingAction)
            {
                _actionWaitCounter += Time.deltaTime;
                _creature.GetAgent().isStopped = true;
                Debug.Log(_isPerformingAction + " " + _actionWaitCounter + " " + _actionTime);
                if (_actionWaitCounter >= _actionTime)
                {
                    _isPerformingAction = false;
                    _action?.Invoke();
                    ClearData("target");
                    _creature.GetAgent().isStopped = false;
                    Debug.Log("TaskAction: Success - Action performed");
                    State = NodeState.Success;
                    return State;
                }

                State = NodeState.Running;
                return State;
            }

            // If _isPerformingAction is false, it means the action has already been performed.
            // So, we return the current state.
            State = NodeState.Success;
            return State;
        }
    }
}